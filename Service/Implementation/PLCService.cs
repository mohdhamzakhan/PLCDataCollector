using CsvHelper;
using FluentFTP;
using Modbus;
using Modbus.Device;
using Newtonsoft.Json;
using PLCDataCollector.Model.Classes;
using PLCDataCollector.Service.Interfaces;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

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
                var rawData = await ReadFromFTPAsync(lineDetail.PLC);

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
            var result = new Dictionary<string, object>();

            try
            {
                if (plcConfig != null)
                {
                    if (plcConfig.FTP.ToLower().Contains("ftp"))
                    {
                        var client = new FtpClient(plcConfig.IP, new NetworkCredential(plcConfig.Username, plcConfig.Password));
                        client.Port = int.Parse(plcConfig.Port);
                        client.Connect();

                        using (var stream = new MemoryStream())
                        {
                            bool success = client.DownloadStream(stream, plcConfig.FilePath);
                            if (!success)
                                throw new Exception("Failed to download FTP file.");

                            stream.Position = 0;
                            using var reader = new StreamReader(stream);
                            var fileContent = await reader.ReadToEndAsync();

                            using var csvReader = new CsvReader(new StringReader(fileContent), CultureInfo.InvariantCulture);


                            // SKIP the configured number of lines before processing
                            for (int i = 0; i < plcConfig.SkipLine; i++)
                            {
                                await csvReader.ReadAsync();
                            }
                            await csvReader.ReadAsync();
                            csvReader.ReadHeader();

                            int rowIndex = 0;
                            while (await csvReader.ReadAsync())
                            {
                                var row = new Dictionary<string, string>();
                                foreach (var header in csvReader.HeaderRecord)
                                    row[header] = csvReader.GetField(header);

                                string jsonString = System.Text.Json.JsonSerializer.Serialize(row);
                                result.Add(rowIndex.ToString(), jsonString);
                                rowIndex++;
                            }
                        }
                    }
                    else
                    {
                        using (var client = new TcpClient(plcConfig.IP, Int32.Parse(plcConfig.Port)))
                        {
                            byte slaveId = 1;
                            var master = ModbusIpMaster.CreateIp(client);

                            Console.WriteLine("✅ Connected to PLC via Modbus TCP");

                            // 📥 Read 5 holding registers starting at address 0
                            ushort startAddress = 0;
                            ushort numRegisters = 5;
                            ushort[] registers = master.ReadHoldingRegisters(slaveId, startAddress, numRegisters);

                            Console.WriteLine("Register values:");
                            for (int i = 0; i < registers.Length; i++)
                            {
                                Console.WriteLine($"Address {startAddress + i}: {registers[i]}");
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read from FTP for PLC {IpAddress}", plcConfig.IP);
                throw;
            }

            return result;
        }


        private int ExtractCount(Dictionary<string, object> rawData, DataLocationConfig dataLocation)
        {
            try
            {
                // Use the dataLocation properties to extract count
                // Example: if count is at a specific position/offset
                if (rawData.ContainsKey("ProductionCount"))
                {
                    return Convert.ToInt32(rawData["ProductionCount"]);
                }

                // If you're extracting from a string at specific positions:
                if (rawData.ContainsKey("DataString"))
                {
                    var dataString = rawData["DataString"].ToString();
                    // Use dataLocation.Part or other properties to determine position
                    var startPos = dataLocation.Part; // or whatever logic you need
                    var length = dataLocation.Lenght; // or specific length for count

                    if (dataString.Length > startPos + length)
                    {
                        var countStr = dataString.Substring(startPos, length);
                        return int.TryParse(countStr.Trim(), out var count) ? count : 0;
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error extracting count from raw data");
                return 0;
            }
        }

        private string ExtractPartNumber(Dictionary<string, object> rawData, DataLocationConfig dataLocation)
        {

            try
            {
                if (rawData.TryGetValue("0", out var entryObj) && entryObj is string jsonString)
                {
                    // Deserialize to Dictionary<string, object>
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
                    if (dict.TryGetValue("QR", out var qr) && qr != null)
                        return qr.ToString().Trim();

                    // Optional: check P2_Type as alternative
                    if (dict.TryGetValue("PartNumber", out var partNum) && partNum != null)
                        return partNum.ToString().Trim();
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error extracting part number from raw data");
                return string.Empty;
            }
        }

        private int ExtractCycleTime(Dictionary<string, object> rawData, DataLocationConfig dataLocation)
        {
            try
            {
                // Use the dataLocation properties to extract cycle time
                if (rawData.ContainsKey("CycleTime"))
                {
                    return Convert.ToInt32(rawData["CycleTime"]);
                }

                // If you're extracting from a string at specific positions:
                if (rawData.ContainsKey("DataString"))
                {
                    var dataString = rawData["DataString"].ToString();
                    // Use dataLocation.Time to determine position
                    var startPos = dataLocation.Time;
                    var length = 5; // or use another property from dataLocation

                    if (dataString.Length > startPos + length)
                    {
                        var cycleTimeStr = dataString.Substring(startPos, length);
                        return int.TryParse(cycleTimeStr.Trim(), out var cycleTime) ? cycleTime : 0;
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error extracting cycle time from raw data");
                return 0;
            }
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
