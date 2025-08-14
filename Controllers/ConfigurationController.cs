using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PLCDataCollector.Model;
using PLCDataCollector.Service.Interfaces;

namespace PLCDataCollector.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigurationController : ControllerBase
    {
        private readonly IConfigurationService _configService;
        private readonly ILogger<ConfigurationController> _logger;

        public ConfigurationController(IConfigurationService configService, ILogger<ConfigurationController> logger)
        {
            _configService = configService;
            _logger = logger;
        }

        /// <summary>
        /// Get all application settings
        /// </summary>
        /// <returns>Application configuration</returns>
        [HttpGet("app-settings")]
        public ActionResult<AppSettings> GetAppSettings()
        {
            try
            {
                var settings = _configService.GetAppSettings();
                return Ok(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get app settings");
                return StatusCode(500, new { error = "Failed to retrieve app settings" });
            }
        }

        /// <summary>
        /// Get current shift information
        /// </summary>
        /// <returns>Current shift details</returns>
        [HttpGet("current-shift")]
        public ActionResult<ShiftDetail> GetCurrentShift()
        {
            try
            {
                var currentShift = _configService.GetCurrentShift();
                if (currentShift == null)
                {
                    return NotFound(new { message = "No current shift found" });
                }
                return Ok(currentShift);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get current shift");
                return StatusCode(500, new { error = "Failed to retrieve current shift" });
            }
        }

        /// <summary>
        /// Get line configuration details
        /// </summary>
        /// <param name="lineKey">Line configuration key</param>
        /// <returns>Line configuration details</returns>
        [HttpGet("line/{lineKey}")]
        public ActionResult<LineDetail> GetLineDetail(string lineKey)
        {
            try
            {
                var lineDetail = _configService.GetLineDetail(lineKey);
                if (lineDetail == null)
                {
                    return NotFound(new { message = $"Line configuration not found for key: {lineKey}" });
                }
                return Ok(lineDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get line detail for key {LineKey}", lineKey);
                return StatusCode(500, new { error = "Failed to retrieve line configuration" });
            }
        }

        /// <summary>
        /// Get all available lines
        /// </summary>
        /// <returns>List of all configured production lines</returns>
        [HttpGet("lines")]
        public ActionResult GetAllLines()
        {
            try
            {
                var settings = _configService.GetAppSettings();
                var lines = settings.LineDetails.Select(kvp => new
                {
                    Key = kvp.Key,
                    LineName = kvp.Value.LineName,
                    LineId = kvp.Value.LineId,
                    LineType = kvp.Value.LineType
                }).ToList();

                return Ok(lines);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all lines");
                return StatusCode(500, new { error = "Failed to retrieve line configurations" });
            }
        }

        /// <summary>
        /// Get system status and configuration summary
        /// </summary>
        /// <returns>System status information</returns>
        [HttpGet("system-status")]
        public ActionResult GetSystemStatus()
        {
            try
            {
                var currentShift = _configService.GetCurrentShift();
                var updateFrequency = _configService.GetUpdateFrequency();
                var timeZone = _configService.GetCurrentTimeZone();
                var liveMetricsEnabled = _configService.IsLiveMetricsEnabled();

                var status = new
                {
                    Timestamp = DateTime.Now,
                    TimeZone = timeZone,
                    CurrentShift = currentShift?.Name ?? "Unknown",
                    UpdateFrequency = updateFrequency,
                    LiveMetricsEnabled = liveMetricsEnabled,
                    SystemUptime = Environment.TickCount64 / 1000, // seconds
                    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
                };

                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get system status");
                return StatusCode(500, new { error = "Failed to retrieve system status" });
            }
        }
    }
}
