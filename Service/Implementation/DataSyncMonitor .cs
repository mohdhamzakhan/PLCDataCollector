using PLCDataCollector.Model.Classes;
using PLCDataCollector.Service.Interfaces;
using System.Collections.Concurrent;

namespace PLCDataCollector.Service.Implementation
{
    public class DataSyncMonitor : IDataSyncMonitor
    {
        private readonly ILogger<DataSyncMonitor> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfigurationService _configService;

        // Thread-safe collections for tracking sync status
        private readonly ConcurrentDictionary<string, SyncStatus> _syncStatuses = new();
        private readonly ConcurrentDictionary<string, DateTime> _lastSyncAttempts = new();
        private readonly ConcurrentDictionary<string, int> _retryCounts = new();
        private readonly ConcurrentDictionary<string, string> _lastErrors = new();

        public DataSyncMonitor(
            ILogger<DataSyncMonitor> logger,
            IServiceProvider serviceProvider,
            IConfigurationService configService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));

            _logger.LogInformation("DataSyncMonitor initialized");
        }
        public async Task<SyncStatus> GetSyncStatusAsync(string lineId)
        {
            if (string.IsNullOrWhiteSpace(lineId))
            {
                throw new ArgumentException("LineId cannot be null or empty", nameof(lineId));
            }

            try
            {
                _logger.LogDebug("Getting sync status for line {LineId}", lineId);

                // Get or create sync status for the line
                var syncStatus = await GetOrCreateSyncStatusAsync(lineId);

                _logger.LogDebug("Sync status for line {LineId}: InSync={IsInSync}, Pending={PendingRecords}",
                    lineId, syncStatus.IsInSync, syncStatus.PendingRecords);

                return syncStatus;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sync status for line {LineId}", lineId);

                // Return a status indicating error
                return new SyncStatus
                {
                    LineId = lineId,
                    LastSyncAttempt = _lastSyncAttempts.GetValueOrDefault(lineId, DateTime.MinValue),
                    IsInSync = false,
                    PendingRecords = -1, // Indicate unknown
                    RetryCount = _retryCounts.GetValueOrDefault(lineId, 0),
                    LastError = $"Error getting status: {ex.Message}"
                };
            }
        }

        public async Task<IEnumerable<SyncStatus>> GetAllSyncStatusAsync()
        {
            try
            {
                _logger.LogDebug("Getting sync status for all lines");

                var appSettings = _configService.GetAppSettings();
                var lineDetails = appSettings?.LineDetails ?? new Dictionary<string, LineDetail>();

                if (!lineDetails.Any())
                {
                    _logger.LogWarning("No line details configured");
                    return Enumerable.Empty<SyncStatus>();
                }

                var syncStatuses = new List<SyncStatus>();

                foreach (var line in lineDetails)
                {
                    try
                    {
                        var syncStatus = await GetOrCreateSyncStatusAsync(line.Key);
                        syncStatuses.Add(syncStatus);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error getting sync status for line {LineId}", line.Key);

                        // Add error status for this line
                        syncStatuses.Add(new SyncStatus
                        {
                            LineId = line.Key,
                            LastSyncAttempt = _lastSyncAttempts.GetValueOrDefault(line.Key, DateTime.MinValue),
                            IsInSync = false,
                            PendingRecords = -1,
                            RetryCount = _retryCounts.GetValueOrDefault(line.Key, 0),
                            LastError = $"Error getting status: {ex.Message}"
                        });
                    }
                }

                _logger.LogInformation("Retrieved sync status for {Count} lines", syncStatuses.Count);
                return syncStatuses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sync status for all lines");
                return Enumerable.Empty<SyncStatus>();
            }
        }

        private async Task<SyncStatus> GetOrCreateSyncStatusAsync(string lineId)
        {
            // Try to get existing status first
            if (_syncStatuses.TryGetValue(lineId, out var existingStatus))
            {
                // Update the status with current information
                await UpdateSyncStatusAsync(existingStatus);
                return existingStatus;
            }

            // Create new status
            var newStatus = await CreateSyncStatusAsync(lineId);
            _syncStatuses[lineId] = newStatus;

            return newStatus;
        }

        private async Task<SyncStatus> CreateSyncStatusAsync(string lineId)
        {
            var syncStatus = new SyncStatus
            {
                LineId = lineId,
                LastSyncAttempt = _lastSyncAttempts.GetValueOrDefault(lineId, DateTime.MinValue),
                IsInSync = false,
                PendingRecords = 0,
                RetryCount = _retryCounts.GetValueOrDefault(lineId, 0),
                LastError = _lastErrors.GetValueOrDefault(lineId, string.Empty)
            };

            await UpdateSyncStatusAsync(syncStatus);
            return syncStatus;
        }

        private async Task UpdateSyncStatusAsync(SyncStatus syncStatus)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var plcDataService = scope.ServiceProvider.GetRequiredService<IPlcDataService>();

                // Get pending records count
                var unsyncedData = await plcDataService.GetUnsyncedDataAsync(syncStatus.LineId);
                var pendingCount = unsyncedData?.Count() ?? 0;

                // Update sync status
                syncStatus.PendingRecords = pendingCount;
                syncStatus.IsInSync = pendingCount == 0 && string.IsNullOrEmpty(syncStatus.LastError);

                // Update last sync attempt if we have data
                var lastAttempt = _lastSyncAttempts.GetValueOrDefault(syncStatus.LineId);
                if (lastAttempt != DateTime.MinValue)
                {
                    syncStatus.LastSyncAttempt = lastAttempt;
                }

                _logger.LogDebug("Updated sync status for line {LineId}: Pending={PendingRecords}, InSync={IsInSync}",
                    syncStatus.LineId, syncStatus.PendingRecords, syncStatus.IsInSync);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating sync status for line {LineId}", syncStatus.LineId);
                syncStatus.LastError = $"Update error: {ex.Message}";
                syncStatus.IsInSync = false;
            }
        }

        // Methods to be called by DataSyncBackgroundService to update status
        public void UpdateSyncAttempt(string lineId, DateTime attemptTime, bool success, string error = null)
        {
            _lastSyncAttempts[lineId] = attemptTime;

            if (!success && !string.IsNullOrEmpty(error))
            {
                _lastErrors[lineId] = error;
                _retryCounts.TryGetValue(lineId, out var currentRetries);
                _retryCounts[lineId] = currentRetries + 1;

                _logger.LogWarning("Sync attempt failed for line {LineId}: {Error}. Retry count: {RetryCount}",
                    lineId, error, currentRetries + 1);
            }
            else if (success)
            {
                _lastErrors.TryRemove(lineId, out _);
                _retryCounts.TryRemove(lineId, out _);

                _logger.LogDebug("Sync attempt succeeded for line {LineId}", lineId);
            }

            // Update cached status if it exists
            if (_syncStatuses.TryGetValue(lineId, out var status))
            {
                status.LastSyncAttempt = attemptTime;
                status.RetryCount = _retryCounts.GetValueOrDefault(lineId, 0);
                status.LastError = _lastErrors.GetValueOrDefault(lineId, string.Empty);
                status.IsInSync = success && status.PendingRecords == 0;
            }
        }

        public void ClearRetryCount(string lineId)
        {
            _retryCounts.TryRemove(lineId, out _);
            _lastErrors.TryRemove(lineId, out _);

            if (_syncStatuses.TryGetValue(lineId, out var status))
            {
                status.RetryCount = 0;
                status.LastError = string.Empty;
            }

            _logger.LogDebug("Cleared retry count for line {LineId}", lineId);
        }

        // Method to get overall health status
        public bool IsHealthy()
        {
            try
            {
                var appSettings = _configService.GetAppSettings();
                var lineDetails = appSettings?.LineDetails ?? new Dictionary<string, LineDetail>();

                if (!lineDetails.Any())
                {
                    return true; // No lines configured, consider healthy
                }

                // Check if any line has excessive retries or old sync attempts
                var now = DateTime.Now;
                var staleThreshold = TimeSpan.FromMinutes(30); // Consider stale if no sync in 30 minutes

                foreach (var line in lineDetails)
                {
                    var lineId = line.Key;
                    var retryCount = _retryCounts.GetValueOrDefault(lineId, 0);
                    var lastAttempt = _lastSyncAttempts.GetValueOrDefault(lineId, DateTime.MinValue);

                    // Check for excessive retries
                    if (retryCount > 10)
                    {
                        _logger.LogWarning("Line {LineId} has excessive retry count: {RetryCount}", lineId, retryCount);
                        return false;
                    }

                    // Check for stale sync attempts
                    if (lastAttempt != DateTime.MinValue && (now - lastAttempt) > staleThreshold)
                    {
                        _logger.LogWarning("Line {LineId} has stale sync attempt: {LastAttempt}", lineId, lastAttempt);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking health status");
                return false;
            }
        }

        // Method to get summary statistics
        public Dictionary<string, object> GetSummary()
        {
            try
            {
                var appSettings = _configService.GetAppSettings();
                var lineDetails = appSettings?.LineDetails ?? new Dictionary<string, LineDetail>();
                var totalLines = lineDetails.Count;

                var linesWithErrors = _lastErrors.Count;
                var totalRetries = _retryCounts.Values.Sum();
                var recentSyncs = _lastSyncAttempts.Values.Count(time =>
                    time > DateTime.MinValue && (DateTime.Now - time) < TimeSpan.FromMinutes(30));

                return new Dictionary<string, object>
                {
                    ["TotalLines"] = totalLines,
                    ["LinesWithErrors"] = linesWithErrors,
                    ["TotalRetries"] = totalRetries,
                    ["RecentSyncs"] = recentSyncs,
                    ["IsHealthy"] = IsHealthy(),
                    ["LastUpdated"] = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting summary");
                return new Dictionary<string, object>
                {
                    ["Error"] = ex.Message,
                    ["LastUpdated"] = DateTime.Now
                };
            }
        }
    }
}
