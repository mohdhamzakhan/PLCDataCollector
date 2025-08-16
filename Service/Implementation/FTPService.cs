using PLCDataCollector.Model.Classes;
using PLCDataCollector.Service.Interfaces;
using System.Net;

namespace PLCDataCollector.Service.Implementation
{


    public class FTPService : IFTPService
    {
        private readonly ILogger<FTPService> _logger;

        public FTPService(ILogger<FTPService> logger)
        {
            _logger = logger;
        }

        public async Task<string> ReadFileAsync(PLCConfig config)
        {
            try
            {
                var request = (FtpWebRequest)WebRequest.Create($"{config.FTP}{config.IP}:{config.Port}{config.FilePath}");
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.Credentials = new NetworkCredential(config.Username, config.Password);
                request.UseBinary = false;
                request.UsePassive = true;
                request.KeepAlive = false;

                using var response = (FtpWebResponse)await request.GetResponseAsync();
                using var responseStream = response.GetResponseStream();
                using var reader = new StreamReader(responseStream);

                var content = await reader.ReadToEndAsync();
                _logger.LogDebug("Successfully read FTP file from {IP}:{Port}", config.IP, config.Port);
                return content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read FTP file from {IP}:{Port}", config.IP, config.Port);
                throw;
            }
        }

        public async Task<bool> TestConnectionAsync(PLCConfig config)
        {
            try
            {
                var request = (FtpWebRequest)WebRequest.Create($"{config.FTP}{config.IP}:{config.Port}/");
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential(config.Username, config.Password);
                request.Timeout = 10000; // 10 seconds timeout

                using var response = (FtpWebResponse)await request.GetResponseAsync();
                return response.StatusCode == FtpStatusCode.OpeningData;
            }
            catch
            {
                return false;
            }
        }
    }
}
