using CsvHelper;
using PLCDataCollector.Model.Classes;
using System.Formats.Asn1;
using System.Globalization;
using System.Text.Json;

namespace PLCDataCollector.Model.Helper
{
    public static class CSVHelper
    {
        public static async Task<List<targetPLCData>> ParseAndSerializeCsvAsync(string csvContent, string lineId)
        {
            var result = new List<targetPLCData>();

            using var reader = new StringReader(csvContent);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            await csv.ReadAsync();
            csv.ReadHeader();

            while (await csv.ReadAsync())
            {
                var row = new Dictionary<string, string>();
                foreach (var header in csv.HeaderRecord)
                {
                    row[header] = csv.GetField(header);
                }

                string jsonString = JsonSerializer.Serialize(row);

                result.Add(new targetPLCData
                {
                    LineId = lineId,
                    Data = jsonString,
                    SyncStatus = 0,
                    Timestamp = DateTime.Now
                });
            }
            return result;
        }
    }
}
