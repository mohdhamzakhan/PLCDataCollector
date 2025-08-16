namespace PLCDataCollector.Service.Interfaces
{
    public interface IPlcDataService
    {
        Task SavePlcDataAsync(string lineId, Dictionary<string, object> data);
        Task<bool> UpdateSyncStatusAsync(string lineId, int recordId);
        Task<IEnumerable<Dictionary<string, object>>> GetUnsyncedDataAsync(string lineId);
    }
}
