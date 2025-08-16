using FluentMigrator.Runner;
using Microsoft.Data.Sqlite;
using Oracle.ManagedDataAccess.Client;
using PLCDataCollector.Model.Database;
using PLCDataCollector.Model.Exceptions;
using PLCDataCollector.Service.Interfaces;

namespace PLCDataCollector.Service.Implementation
{
    public class DatabaseMigrationService : IDisposable
    {
        private readonly ILogger<DatabaseMigrationService> _logger;
        private readonly ISourceDatabaseContext _sourceDb;
        private readonly ITargetDatabaseContext _targetDb;
        private readonly IConfiguration _configuration;
        private readonly List<IDisposable> _disposables = new();


        public DatabaseMigrationService(
            ILogger<DatabaseMigrationService> logger,
            ISourceDatabaseContext sourceDb,
            ITargetDatabaseContext targetDb,
            IConfiguration configuration)
        {
            _logger = logger;
            _sourceDb = sourceDb;
            _targetDb = targetDb;
            _configuration = configuration;
        }

        public async Task MigrateAsync()
        {
            try
            {
                //await MigrateSourceDatabaseAsync();
                //await MigrateTargetDatabaseAsync();
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to perform database migrations");
                throw new DatabaseException("Migration failed", "Migration", "Both", ex);
            }
        }

        private async Task MigrateSourceDatabaseAsync()
        {
            //var sourceScript = await File.ReadAllTextAsync("Model/Database/init.sql");
            //using var conn = _sourceDb.CreateConnection();
            //conn.Open();

            //using var cmd = conn.CreateCommand();
            //cmd.CommandText = sourceScript;
            //cmd.ExecuteNonQuery();

            //_logger.LogInformation("Source database migration completed");
            return;
        }

        private async Task MigrateTargetDatabaseAsync()
        {
            var env = _configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") ?? "Development";
            var dbType = _configuration.GetValue<string>($"DatabaseSettings:TargetType:{env}");

            if (dbType?.ToUpper() == "ORACLE")
            {
                await MigrateOracleTargetAsync();
            }
            else
            {
                await MigrateSqliteTargetAsync();
            }
        }

        private async Task MigrateOracleTargetAsync()
        {
            var oracleScript = await File.ReadAllTextAsync("Model/Database/oracle_init.sql");
            using var conn = _targetDb.CreateConnection() as OracleConnection;
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = oracleScript;
            await cmd.ExecuteNonQueryAsync();

            _logger.LogInformation("Oracle target database migration completed");
        }

        private async Task MigrateSqliteTargetAsync()
        {
            var sqliteScript = await File.ReadAllTextAsync("Model/Database/init.sql");
            using var conn = _targetDb.CreateConnection() as SqliteConnection;
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = sqliteScript;
            await cmd.ExecuteNonQueryAsync();

            _logger.LogInformation("SQLite target database migration completed");
        }

        public void Dispose()
        {
            foreach (var disposable in _disposables)
            {
                disposable?.Dispose();
            }
            _disposables.Clear();
        }
    }
}