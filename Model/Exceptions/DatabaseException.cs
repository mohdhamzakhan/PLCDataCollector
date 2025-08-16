namespace PLCDataCollector.Model.Exceptions
{
    public class DatabaseException : Exception
    {
        public string Operation { get; }
        public string DatabaseType { get; }

        public DatabaseException(string message, string operation, string databaseType, Exception innerException = null)
            : base(message, innerException)
        {
            Operation = operation;
            DatabaseType = databaseType;
        }
    }

    public class DataSyncException : DatabaseException
    {
        public string LineId { get; }
        public int? RecordId { get; }

        public DataSyncException(string message, string lineId, int? recordId = null, Exception innerException = null)
            : base(message, "Sync", "Both", innerException)
        {
            LineId = lineId;
            RecordId = recordId;
        }
    }
}