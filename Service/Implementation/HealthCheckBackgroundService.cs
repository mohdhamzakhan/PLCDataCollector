using PLCDataCollector.Service.Interfaces;

namespace PLCDataCollector.Service.Implementation
{
    public class HealthCheckBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<HealthCheckBackgroundService> _logger;

        public HealthCheckBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<HealthCheckBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Health Check Background Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var configService = scope.ServiceProvider.GetRequiredService<IConfigurationService>();
                    var plcService = scope.ServiceProvider.GetRequiredService<IPLCService>();

                    var appSettings = configService.GetAppSettings();

                    foreach (var lineDetail in appSettings.LineDetails)
                    {
                        try
                        {
                            var isConnected = await plcService.TestConnectionAsync(lineDetail.Key);
                            if (!isConnected)
                            {
                                _logger.LogWarning("PLC connection failed for line {LineId}", lineDetail.Key);
                            }
                            else
                            {
                                _logger.LogDebug("PLC connection healthy for line {LineId}", lineDetail.Key);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Health check failed for line {LineId}", lineDetail.Key);
                        }
                    }

                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Check every 5 minutes
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in health check background service");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Wait 1 minute on error
                }
            }
        }
    }
}
