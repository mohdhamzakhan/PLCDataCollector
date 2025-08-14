using PLCDataCollector.Service.Interfaces;

namespace PLCDataCollector.Service.Implementation
{
    public class PLCDataCollectorBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PLCDataCollectorBackgroundService> _logger;
        private readonly IConfigurationService _configService;

        public PLCDataCollectorBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<PLCDataCollectorBackgroundService> logger,
            IConfigurationService configService)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configService = configService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var updateFrequency = _configService.GetUpdateFrequency();
            var delayMs = updateFrequency * 1000; // Convert seconds to milliseconds

            _logger.LogInformation($"Starting PLC data collection with {updateFrequency}s update frequency in {_configService.GetCurrentTimeZone()} timezone");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var productionService = scope.ServiceProvider.GetRequiredService<IProductionService>();
                    var graphDataService = scope.ServiceProvider.GetRequiredService<IGraphDataService>();
                    var webSocketService = scope.ServiceProvider.GetRequiredService<IWebSocketService>();
                    var alertService = scope.ServiceProvider.GetRequiredService<IAlertService>();

                    // Only collect if live metrics are enabled
                    if (_configService.IsLiveMetricsEnabled())
                    {
                        await CollectAndBroadcastDataAsync(productionService, graphDataService, webSocketService, alertService);
                    }

                    await Task.Delay(delayMs, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in PLC data collection");
                    await Task.Delay(delayMs * 2, stoppingToken); // Wait double time on error
                }
            }
        }

        private async Task CollectAndBroadcastDataAsync(
            IProductionService productionService,
            IGraphDataService graphDataService,
            IWebSocketService webSocketService,
            IAlertService alertService)
        {
            var appSettings = _configService.GetAppSettings();

            foreach (var lineDetail in appSettings.LineDetails)
            {
                try
                {
                    // Get real-time production data
                    var productionData = await productionService.GetRealTimeProductionAsync(lineDetail.Key);

                    // Get real-time graph data
                    var graphData = await graphDataService.GetRealTimeGraphDataAsync(lineDetail.Key);

                    // Broadcast production data via WebSocket
                    await webSocketService.BroadcastProductionDataAsync(lineDetail.Key, productionData);

                    // Broadcast graph data via WebSocket
                    await webSocketService.BroadcastGraphDataAsync(lineDetail.Key, graphData);

                    // Check for alerts
                    var shiftStatus = await productionService.GetCurrentShiftStatusAsync(lineDetail.Key);
                    foreach (var alert in shiftStatus.Alerts)
                    {
                        await alertService.AddAlertAsync(alert);
                        await webSocketService.BroadcastAlertAsync(alert);
                    }

                    _logger.LogDebug("Collected and broadcasted data for line {LineId}: {Count} pieces",
                        lineDetail.Key, productionData.ActualCount);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to collect data for line {LineId}", lineDetail.Key);
                }
            }
        }
    }
}
