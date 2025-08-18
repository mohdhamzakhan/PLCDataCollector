using Dapper;
using Microsoft.Extensions.Options;
using PLCDataCollector.Model.Classes;
using PLCDataCollector.Model.Database;
using PLCDataCollector.Model.Exceptions;
using PLCDataCollector.Service.Interfaces;
using System.Collections.Concurrent;
using System.Data;

namespace PLCDataCollector.Service.Implementation
{
    public class DataSyncBackgroundService : BackgroundService
    {
        private readonly ILogger<DataSyncBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly DataSyncSettings _settings;
        private readonly IConfigurationService _configService;
        private readonly ConcurrentDictionary<string, DateTime> _lastSyncTimes = new();
        private readonly ConcurrentDictionary<string, int> _retryCount = new();

        public DataSyncBackgroundService(
            ILogger<DataSyncBackgroundService> logger,
            IServiceProvider serviceProvider,
            IOptions<DataSyncSettings> settings,
            IConfigurationService configService)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _settings = settings.Value;
            _configService = configService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Data Sync Background Service starting at: {time}", DateTimeOffset.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SyncAllLinesDataAsync(stoppingToken);
                    await Task.Delay(TimeSpan.FromSeconds(_settings.IntervalSeconds), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Data Sync Background Service stopping");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in Data Sync Background Service");
                    await Task.Delay(TimeSpan.FromSeconds(_settings.RetryDelaySeconds), stoppingToken);
                }
            }
        }

        private async Task SyncAllLinesDataAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var plcDataService = scope.ServiceProvider.GetRequiredService<IPlcDataService>();

            // Get all configured production lines
            var lineDetails = _configService.GetAppSettings().LineDetails;

            foreach (var line in lineDetails)
            {
                if (stoppingToken.IsCancellationRequested) break;

                var lineId = line.Key;
                try
                {
                    await SyncLineDataAsync(plcDataService, lineId, stoppingToken);
                    _retryCount.TryRemove(lineId, out _);
                }
                catch (Exception ex)
                {
                    await HandleSyncErrorAsync(lineId, ex);
                }
            }
        }

        private async Task SyncLineDataAsync(IPlcDataService plcDataService, string lineId, CancellationToken stoppingToken)
        {
            if (_settings.EnableDetailedLogging)
            {
                _logger.LogInformation("Starting data sync for line {LineId} at {Time}",
                    lineId, DateTimeOffset.Now);
            }

            var unsyncedData = await plcDataService.GetUnsyncedDataAsync(lineId);
            var dataList = unsyncedData.Take(_settings.BatchSize).ToList();

            if (!dataList.Any())
            {
                if (_settings.EnableDetailedLogging)
                {
                    _logger.LogDebug("No unsynced data found for line {LineId}", lineId);
                }
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var targetDb = scope.ServiceProvider.GetRequiredService<IDatabaseContext>();

            using var connection = targetDb.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                foreach (var data in dataList)
                {
                    if (stoppingToken.IsCancellationRequested) break;

                    var recordId = Convert.ToInt32(data["Id"]);
                    await SyncRecordAsync(connection, data, lineId, stoppingToken);
                    await plcDataService.UpdateSyncStatusAsync(lineId, recordId);

                    if (_settings.EnableDetailedLogging)
                    {
                        _logger.LogDebug("Successfully synced record {RecordId} for line {LineId}",
                            recordId, lineId);
                    }
                }

                transaction.Commit();
                _lastSyncTimes[lineId] = DateTime.Now;

                _logger.LogInformation(
                    "Successfully synced {Count} records for line {LineId} at {Time}",
                    dataList.Count, lineId, DateTime.Now);
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }

        private async Task SyncRecordAsync(IDbConnection connection, Dictionary<string, object> data, string lineId, CancellationToken stoppingToken)
        {
            var sql = @"
                INSERT INTO PlcData (LineId, Data, Timestamp) 
                VALUES (@LineId, @Data, @Timestamp)";

            var parameters = new
            {
                LineId = lineId,
                Data = System.Text.Json.JsonSerializer.Serialize(data["Data"]),
                Timestamp = data["Timestamp"]
            };

            await connection.ExecuteAsync(sql, parameters);
        }

        private async Task HandleSyncErrorAsync(string lineId, Exception ex)
        {
            _retryCount.TryGetValue(lineId, out var currentRetries);
            currentRetries++;

            if (currentRetries <= _settings.MaxRetries)
            {
                _retryCount[lineId] = currentRetries;
                _logger.LogWarning(ex,
                    "Sync failed for line {LineId}. Attempt {Current} of {Max}",
                    lineId, currentRetries, _settings.MaxRetries);

                // Exponential backoff for retries
                await Task.Delay(TimeSpan.FromSeconds(_settings.RetryDelaySeconds * Math.Pow(2, currentRetries - 1)));
            }
            else
            {
                _logger.LogError(ex,
                    "Sync failed for line {LineId} after {MaxRetries} attempts",
                    lineId, _settings.MaxRetries);
                _retryCount.TryRemove(lineId, out _);

                throw new DataSyncException(
                    $"Sync failed after {_settings.MaxRetries} attempts",
                    lineId,
                    null,
                    ex);
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Data Sync Background Service stopping at: {time}", DateTimeOffset.Now);
            await base.StopAsync(stoppingToken);
        }
    }
}