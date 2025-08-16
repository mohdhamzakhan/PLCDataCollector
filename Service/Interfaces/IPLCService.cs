using PLCDataCollector.Model.Classes;

namespace PLCDataCollector.Service.Interfaces
{
    public interface IPLCService
    {
        Task<PLCData> ReadPLCDataAsync(string lineId);
        Task<bool> TestConnectionAsync(string lineId);
        Task<Dictionary<string, object>> ReadRawDataAsync(string lineId);
    }
}
