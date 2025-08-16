using PLCDataCollector.Model.Classes;

namespace PLCDataCollector.Service.Interfaces
{
    public interface IDataParsingService
    {
        Dictionary<string, object> ParsePLCData(string rawData, DataLocationConfig locationConfig);
        int ExtractProductionCount(string rawData, DataLocationConfig locationConfig);
        string ExtractPartNumber(string rawData, DataLocationConfig locationConfig);
        int ExtractCycleTime(string rawData, DataLocationConfig locationConfig);
    }
}
