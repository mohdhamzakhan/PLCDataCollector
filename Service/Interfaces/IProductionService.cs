using PLCDataCollector.Model;

namespace PLCDataCollector.Service.Interfaces
{
    public interface IProductionService
    {
        Task<ProductionData> GetRealTimeProductionAsync(string lineId);
        Task<List<ProductionData>> GetShiftProductionAsync(string lineId, DateTime date, string shift);
        Task<ShiftStatus> GetCurrentShiftStatusAsync(string lineId);
        Task SaveProductionPlanAsync(ProductionPlan plan);
        Task CollectRealTimeDataAsync();
    }
}
