using PLCDataCollector.Model.Classes;
using PLCDataCollector.Service.Interfaces;

namespace PLCDataCollector.Service.Implementation
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IConfiguration _configuration;
        private readonly AppSettings _appSettings;
        private readonly ILogger<ConfigurationService> _logger;

        public ConfigurationService(IConfiguration configuration, ILogger<ConfigurationService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _appSettings = new AppSettings();

            try
            {
                // Bind configuration with more explicit options
                _configuration.Bind(_appSettings, options =>
                {
                    options.BindNonPublicProperties = false;
                    options.ErrorOnUnknownConfiguration = false;
                });

                _logger.LogInformation("Configuration loaded successfully");
                _logger.LogDebug("Found {LineCount} line configurations", _appSettings.LineDetails?.Count ?? 0);

                // Log line details for debugging
                if (_appSettings.LineDetails != null)
                {
                    foreach (var line in _appSettings.LineDetails)
                    {
                        _logger.LogDebug("Line: {LineKey} - {LineName}", line.Key, line.Value?.LineName ?? "Unknown");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to bind configuration");
                throw;
            }
        }

        public AppSettings GetAppSettings() => _appSettings;

        public int GetUpdateFrequency()
        {
            try
            {
                if (_appSettings.RealTimeSettings != null)
                    return _appSettings.RealTimeSettings.UpdateFrequency;

                // Fallback to direct configuration read
                var frequency = _configuration.GetValue<int>("RealTimeSettings:UpdateFrequency");
                return frequency > 0 ? frequency : 5;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get update frequency, using default: 5");
                return 5;
            }
        }

        public string GetCurrentTimeZone()
        {
            try
            {
                return _appSettings.RealTimeSettings?.CurrentTimeZone ??
                       _configuration.GetValue<string>("RealTimeSettings:CurrentTimeZone") ??
                       "IST";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get timezone, using default: IST");
                return "IST";
            }
        }

        public bool IsLiveMetricsEnabled()
        {
            try
            {
                return _appSettings.RealTimeSettings?.ShowLiveMetrics ??
                       _configuration.GetValue<bool>("RealTimeSettings:ShowLiveMetrics", true);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get live metrics setting, using default: true");
                return true;
            }
        }

        public LineDetail GetLineDetail(string lineKey)
        {
            try
            {
                if (_appSettings.LineDetails != null && _appSettings.LineDetails.TryGetValue(lineKey, out var lineDetail))
                {
                    _logger.LogDebug("Found line detail for {LineKey}", lineKey);
                    return lineDetail;
                }

                // Try direct configuration access as fallback
                var section = _configuration.GetSection($"LineDetails:{lineKey}");
                if (section.Exists())
                {
                    var fallbackLineDetail = new LineDetail();
                    section.Bind(fallbackLineDetail);
                    _logger.LogDebug("Found line detail via direct binding for {LineKey}", lineKey);
                    return fallbackLineDetail;
                }

                _logger.LogWarning("No line detail found for {LineKey}", lineKey);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get line detail for {LineKey}", lineKey);
                return null;
            }
        }

        public ShiftDetail GetCurrentShift()
        {
            try
            {
                var timeZone = GetCurrentTimeZone();
                var currentTime = timeZone == "IST"
                    ? DateTime.Now.TimeOfDay
                    : DateTime.Now.TimeOfDay;

                var egrv = GetLineDetail("EGRV_Final");

                if (egrv?.ShiftConfiguration != null)
                {
                    var shiftA = TimeSpan.Parse(egrv.ShiftConfiguration.ShiftA.StartTime);
                    var shiftB = TimeSpan.Parse(egrv.ShiftConfiguration.ShiftB.StartTime);
                    var shiftC = TimeSpan.Parse(egrv.ShiftConfiguration.ShiftC.StartTime);

                    if (currentTime >= shiftA && currentTime < shiftB)
                        return egrv.ShiftConfiguration.ShiftA;
                    else if (currentTime >= shiftB && currentTime < shiftC)
                        return egrv.ShiftConfiguration.ShiftB;
                    else
                        return egrv.ShiftConfiguration.ShiftC;
                }

                _logger.LogWarning("No shift configuration found for EGRV_Final");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get current shift");
                return null;
            }
        }

        // Helper method to get connection string based on environment
        public string GetConnectionString(string name, string environment = null)
        {
            try
            {
                environment ??= Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

                if (name.Equals("Source", StringComparison.OrdinalIgnoreCase))
                {
                    return environment == "Development"
                        ? _appSettings.ConnectionStrings.SourceDatabase.Development
                        : _appSettings.ConnectionStrings.SourceDatabase.Production;
                }
                else if (name.Equals("Target", StringComparison.OrdinalIgnoreCase))
                {
                    return environment == "Development"
                        ? _appSettings.ConnectionStrings.TargetDatabase.Development
                        : _appSettings.ConnectionStrings.TargetDatabase.Production;
                }

                // Fallback to standard connection strings
                return _configuration.GetConnectionString(name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get connection string for {Name}", name);
                return null;
            }
        }

        // Helper method to get database type
        public string GetDatabaseType(string environment = null)
        {
            try
            {
                environment ??= Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

                return environment == "Development"
                    ? _appSettings.DatabaseSettings?.SourceType?.Development ?? "SQLite"
                    : _appSettings.DatabaseSettings?.SourceType?.Production ?? "Oracle";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get database type for environment {Environment}", environment);
                return "SQLite";
            }
        }
    }
}