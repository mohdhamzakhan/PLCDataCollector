namespace PLCDataCollector.Model.Classes
{
    // Add DataSyncSettings if not already defined elsewhere
    public class DataSyncSettings
    {
        public int IntervalSeconds { get; set; } = 30;
        public int BatchSize { get; set; } = 100;
        public int MaxRetries { get; set; } = 3;
        public int RetryDelaySeconds { get; set; } = 5;
        public bool EnableDetailedLogging { get; set; } = true;
    }
}
