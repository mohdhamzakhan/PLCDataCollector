using System.Data;

namespace PLCDataCollector.Model.Database
{
    public interface IDatabaseContext
    {
        IDbConnection CreateConnection();
        bool TestConnectionAsync();
        string ConnectionString { get; }
    }
}
