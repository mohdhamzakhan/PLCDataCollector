using PLCDataCollector.Model;
using PLCDataCollector.Service.Interfaces;

namespace PLCDataCollector.Service.Implementation
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IConfiguration _configuration;
        private readonly AppSettings _appSettings;

        public ConfigurationService(IConfiguration configuration)
        {
            _configuration = configuration;
            _appSettings = new AppSettings();
            _configuration.Bind(_appSettings);
        }

        public AppSettings GetAppSettings() => _appSettings;

        public int GetUpdateFrequency()
        {
            // Handle both old and new structure
            if (_appSettings.RealTimeSettings != null)
                return _appSettings.RealTimeSettings.UpdateFrequency;

            // Fallback to old RefreshTime property
            return _appSettings.RealTimeSettings?.UpdateFrequency > 0 ? _appSettings.RealTimeSettings.UpdateFrequency : 5;
        }

        public string GetCurrentTimeZone()
        {
            // Return timezone from new structure, fallback to IST
            return _appSettings.RealTimeSettings?.CurrentTimeZone ?? "IST";
        }

        public bool IsLiveMetricsEnabled()
        {
            // Default to true if not specified
            return _appSettings.RealTimeSettings?.ShowLiveMetrics ?? true;
        }

        public LineDetail GetLineDetail(string lineKey)
        {
            return _appSettings.LineDetails.TryGetValue(lineKey, out var lineDetail)
                ? lineDetail
                : null;
        }

        public ShiftDetail GetCurrentShift()
        {
            var timeZone = GetCurrentTimeZone();
            var currentTime = timeZone == "IST"
                ? DateTime.Now.TimeOfDay  // Already in IST since you're in India
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

            return null;
        }
    }
}
