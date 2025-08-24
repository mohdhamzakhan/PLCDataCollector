using Dapper;
using PLCDataCollector.Model.Database;
using PLCDataCollector.Service.Interfaces;

namespace PLCDataCollector.Service.Implementation
{
    public class PlcDataService : IPlcDataService
    {
        private readonly IDatabaseContext _sourceDb;
        private readonly ITargetDatabaseContext _targetDb;
        private readonly ILogger<PlcDataService> _logger;

        public PlcDataService(
            IDatabaseContext sourceDb,
            ITargetDatabaseContext targetDb,
            ILogger<PlcDataService> logger)
        {
            _sourceDb = sourceDb;
            _targetDb = targetDb;
            _logger = logger;
        }

        public async Task SavePlcDataAsync(string lineId, Dictionary<string, object> data)
        {
            try
            {
                using var conn = _targetDb.CreateConnection();
                conn.Open();

                const string sql = @"
                    INSERT INTO PlcData (LineId, Data, SyncStatus, Timestamp)
                    VALUES (@LineId, @Data, 0, @Timestamp)
                    RETURNING Id";

                var parameters = new
                {
                    LineId = lineId,
                    Data = System.Text.Json.JsonSerializer.Serialize(data),
                    Timestamp = DateTime.Now
                };

                await conn.ExecuteAsync(sql, parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving PLC data for line {LineId}", lineId);
                throw;
            }
        }

        public async Task<bool> UpdateSyncStatusAsync(string lineId, int recordId)
        {
            try
            {
                using var conn = _targetDb.CreateConnection();
                conn.Open();

                const string sql = @"
                    UPDATE PlcData 
                    SET SyncStatus = 1 
                    WHERE Id = @RecordId AND LineId = @LineId";

                var result = await conn.ExecuteAsync(sql, new { RecordId = recordId, LineId = lineId });
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating sync status for line {LineId}, record {RecordId}", lineId, recordId);
                return false;
            }
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetUnsyncedDataAsync(string lineId)
        {
            try
            {
                using var conn = _targetDb.CreateConnection();
                conn.Open();

                const string sql = @"
                    SELECT Id, Data, Timestamp
                    FROM PlcData
                    WHERE LineId = @LineId AND SyncStatus = 0
                    ORDER BY Timestamp ASC";

                var results = await conn.QueryAsync(sql, new { LineId = lineId });

                return results.Select(r => new Dictionary<string, object>
                {
                    ["Id"] = r.Id,
                    ["Data"] = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(r.Data.ToString()),
                    ["Timestamp"] = r.Timestamp
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unsynced data for line {LineId}", lineId);
                throw;
            }
        }
    }
}