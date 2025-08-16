namespace PLCDataCollector.Service.Interfaces
{
    public interface IDataSyncMonitor
    {
        Task<SyncStatus> GetSyncStatusAsync(string lineId);
        Task<IEnumerable<SyncStatus>> GetAllSyncStatusAsync();
    }

    public class SyncStatus
    {
        public string LineId { get; set; }
        public DateTime LastSyncAttempt { get; set; }
        public bool IsInSync { get; set; }
        public int PendingRecords { get; set; }
        public int RetryCount { get; set; }
        public string LastError { get; set; }
    }
}
