using PLCDataCollector.Model.Classes;

namespace PLCDataCollector.Service.Interfaces
{
    public interface IConfigurationService
    {
        AppSettings GetAppSettings();
        ShiftDetail GetCurrentShift();
        string GetCurrentTimeZone();
        LineDetail GetLineDetail(string lineKey);
        int GetUpdateFrequency();
        bool IsLiveMetricsEnabled();
    }
}
