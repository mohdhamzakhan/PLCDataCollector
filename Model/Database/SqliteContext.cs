using Microsoft.Data.Sqlite;
using System.Data;

namespace PLCDataCollector.Model.Database
{
    public class SqliteContext: IDatabaseContext
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
