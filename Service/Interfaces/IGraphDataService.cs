using PLCDataCollector.Model;

namespace PLCDataCollector.Service.Interfaces
{
    public interface IGraphDataService
    {
        Task<RealTimeGraphData> GetRealTimeGraphDataAsync(string lineId);
        Task<RealTimeGraphData> GetShiftGraphDataAsync(string lineId, DateTime date, string shift);
        Task UpdateGraphDataAsync(string lineId, PLCData plcData);
    }
}
