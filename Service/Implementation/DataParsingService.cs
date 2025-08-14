using PLCDataCollector.Model;
using PLCDataCollector.Service.Interfaces;

namespace PLCDataCollector.Service.Implementation
{
    public class DataParsingService : IDataParsingService
    {
        private readonly ILogger<DataParsingService> _logger;

        public DataParsingService(ILogger<DataParsingService> logger)
        {
            _logger = logger;
        }

        public Dictionary<string, object> ParsePLCData(string rawData, DataLocationConfig locationConfig)
        {
            var parsedData = new Dictionary<string, object>();

            try
            {
                if (string.IsNullOrEmpty(rawData))
                {
                    _logger.LogWarning("Raw data is empty");
                    return parsedData;
                }

                // Handle different data formats
                if (rawData.StartsWith("{") && rawData.EndsWith("}"))
                {
                    // JSON format
                    parsedData = ParseJsonData(rawData);
                }
                else if (rawData.Contains(',') || rawData.Contains(';'))
                {
                    // CSV format
                    parsedData = ParseCsvData(rawData, locationConfig);
                }
                else
                {
                    // Fixed-width format
                    parsedData = ParseFixedWidthData(rawData, locationConfig);
                }

                _logger.LogDebug("Successfully parsed PLC data with {Count} fields", parsedData.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse PLC data");
                throw;
            }

            return parsedData;
        }

        public int ExtractProductionCount(string rawData, DataLocationConfig locationConfig)
        {
            var parsedData = ParsePLCData(rawData, locationConfig);

            if (parsedData.TryGetValue("ProductionCount", out var count))
            {
                return Convert.ToInt32(count);
            }

            // Try to extract from fixed position
            if (rawData.Length > locationConfig.Justifation + 10)
            {
                var countStr = rawData.Substring(locationConfig.Justifation, 10).Trim();
                if (int.TryParse(countStr, out var fixedCount))
                {
                    return fixedCount;
                }
            }

            return 0;
        }

        public string ExtractPartNumber(string rawData, DataLocationConfig locationConfig)
        {
            var parsedData = ParsePLCData(rawData, locationConfig);

            if (parsedData.TryGetValue("PartNumber", out var partNumber))
            {
                return partNumber.ToString();
            }

            // Try to extract from fixed position
            if (rawData.Length > locationConfig.Part_Number + 15)
            {
                return rawData.Substring(locationConfig.Part_Number, 15).Trim();
            }

            return "UNKNOWN";
        }

        public int ExtractCycleTime(string rawData, DataLocationConfig locationConfig)
        {
            var parsedData = ParsePLCData(rawData, locationConfig);

            if (parsedData.TryGetValue("CycleTime", out var cycleTime))
            {
                return Convert.ToInt32(cycleTime);
            }

            return locationConfig.Time; // Default from configuration
        }

        private Dictionary<string, object> ParseJsonData(string jsonData)
        {
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonData)
                       ?? new Dictionary<string, object>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse JSON data");
                return new Dictionary<string, object>();
            }
        }

        private Dictionary<string, object> ParseCsvData(string csvData, DataLocationConfig locationConfig)
        {
            var parsedData = new Dictionary<string, object>();

            try
            {
                var delimiter = csvData.Contains(';') ? ';' : ',';
                var values = csvData.Split(delimiter);

                if (values.Length > 0) parsedData["ProductionCount"] = values[0].Trim();
                if (values.Length > 1) parsedData["PartNumber"] = values[1].Trim();
                if (values.Length > 2) parsedData["CycleTime"] = values[2].Trim();
                if (values.Length > 3) parsedData["Status"] = values[3].Trim();
                if (values.Length > 4) parsedData["Timestamp"] = values[4].Trim();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse CSV data");
            }

            return parsedData;
        }

        private Dictionary<string, object> ParseFixedWidthData(string fixedWidthData, DataLocationConfig locationConfig)
        {
            var parsedData = new Dictionary<string, object>();

            try
            {
                if (fixedWidthData.Length >= locationConfig.Lenght)
                {
                    // Extract based on configuration positions
                    parsedData["ProductionCount"] = ExtractFixedField(fixedWidthData, locationConfig.Justifation, 10);
                    parsedData["PartNumber"] = ExtractFixedField(fixedWidthData, locationConfig.Part_Number, 15);
                    parsedData["CycleTime"] = locationConfig.Time.ToString();
                    parsedData["Status"] = ExtractFixedField(fixedWidthData, locationConfig.Part, 2);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse fixed-width data");
            }

            return parsedData;
        }

        private string ExtractFixedField(string data, int startPosition, int length)
        {
            if (data.Length >= startPosition + length)
            {
                return data.Substring(startPosition, length).Trim();
            }
            return string.Empty;
        }
    }
}
