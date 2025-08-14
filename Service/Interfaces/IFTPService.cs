using PLCDataCollector.Model;

namespace PLCDataCollector.Service.Interfaces
{
    public interface IFTPService
    {
        Task<string> ReadFileAsync(PLCConfig config);
        Task<bool> TestConnectionAsync(PLCConfig config);
    }
}
