using PLCDataCollector.Model.Classes;
using PLCDataCollector.Service.Interfaces;

namespace PLCDataCollector.Service.Implementation
{
    public class PLCService : IPLCService
    {
        private readonly ILogger<PLCService> _logger;
        private readonly IConfigurationService _configService;

        public PLCService(ILogger<PLCService> logger, IConfigurationService configService)
        {
            _logger = logger;
            _configService = configService;
        }

        public async Task<PLCData> ReadPLCDataAsync(string lineId)
        {
            try
            {
                var lineDetail = _configService.GetLineDetail(lineId);
                if (lineDetail == null)
                {
                    throw new ArgumentException($"Line configuration not found for {lineId}");
                }

                // Read from PLC (FTP in this case)
                var rawData = await ReadFromFTPAsync(lineDetail.PLCConfig);

                return new PLCData
                {
                    LineId = lineId,
                    Timestamp = DateTime.Now,
                    CurrentCount = ExtractCount(rawData, lineDetail.Data_Location),
                    PartNumber = ExtractPartNumber(rawData, lineDetail.Data_Location),
                    CycleTime = ExtractCycleTime(rawData, lineDetail.Data_Location),
                    IsRunning = DetermineRunningStatus(rawData),
                    RawData = rawData
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read PLC data for line {LineId}", lineId);
                throw;
            }
        }

        public async Task<bool> TestConnectionAsync(string lineId)
        {
            try
            {
                var data = await ReadPLCDataAsync(lineId);
                return data != null;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Dictionary<string, object>> ReadRawDataAsync(string lineId)
        {
            var data = await ReadPLCDataAsync(lineId);
            return data.RawData;
        }

        private async Task<Dictionary<string, object>> ReadFromFTPAsync(PLCConfig plcConfig)
        {
            try
            {
                // Simulate FTP read - replace with actual FTP implementation
                await Task.Delay(100); // Simulate network delay

                // Generate sample data based on current time for demonstration
                var random = new Random();
                return new Dictionary<string, object>
                {
                    ["ProductionCount"] = random.Next(1, 1000),
                    ["PartNumber"] = $"PART{random.Next(1000, 9999)}",
                    ["CycleTime"] = random.Next(10, 30),
                    ["Status"] = random.Next(0, 2), // 0 = stopped, 1 = running
                    ["Timestamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read from FTP for PLC {IpAddress}", plcConfig.IP);
                throw;
            }
        }

        private int ExtractCount(Dictionary<string, object> rawData, string location)
        {
            if (rawData.TryGetValue("ProductionCount", out var count))
            {
                return Convert.ToInt32(count);
            }
            return 0;
        }

        private string ExtractPartNumber(Dictionary<string, object> rawData, string location)
        {
            if (rawData.TryGetValue("PartNumber", out var partNumber))
            {
                return partNumber.ToString();
            }
            return "UNKNOWN";
        }

        private int ExtractCycleTime(Dictionary<string, object> rawData, string location)
        {
            if (rawData.TryGetValue("CycleTime", out var cycleTime))
            {
                return Convert.ToInt32(cycleTime);
            }
            return 0; // Default from configuration
        }

        private bool DetermineRunningStatus(Dictionary<string, object> rawData)
        {
            if (rawData.TryGetValue("Status", out var status))
            {
                return Convert.ToInt32(status) == 1;
            }
            return false;
        }
    }
}
