using Oracle.ManagedDataAccess.Client;
using PLCDataCollector.Service.Interfaces;
using System.Data;

namespace PLCDataCollector.Model.Database
{
    public class OracleContext : IDatabaseContext, ITargetDatabaseContext
    {
        private readonly string _connectionString;

        public OracleContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public string ConnectionString => _connectionString;

        public IDbConnection CreateConnection()
        {
            return new OracleConnection(_connectionString);
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
