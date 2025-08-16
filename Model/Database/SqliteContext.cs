using Microsoft.Data.Sqlite;
using PLCDataCollector.Service.Interfaces;
using System.Data;

namespace PLCDataCollector.Model.Database
{
    public class SqliteContext: IDatabaseContext, ITargetDatabaseContext
    {
        private readonly string _connectionString;

        public SqliteContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public string ConnectionString => _connectionString;

        public IDbConnection CreateConnection()
        {
            return new SqliteConnection(_connectionString);
        }

        public bool TestConnectionAsync()
        {
            try
            {
                using var connection = CreateConnection();
                connection.Open();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
