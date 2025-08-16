using Microsoft.AspNetCore.Mvc;
using PLCDataCollector.Service.Interfaces;

namespace PLCDataCollector.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataSyncController : ControllerBase
    {
        private readonly IDataSyncMonitor _syncMonitor;
        private readonly ILogger<DataSyncController> _logger;

        public DataSyncController(
            IDataSyncMonitor syncMonitor,
            ILogger<DataSyncController> logger)
        {
            _syncMonitor = syncMonitor;
            _logger = logger;
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetAllSyncStatus()
        {
            try
            {
                var status = await _syncMonitor.GetAllSyncStatusAsync();
                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sync status");
                return StatusCode(500, new { Error = "Failed to retrieve sync status" });
            }
        }

        [HttpGet("status/{lineId}")]
        public async Task<IActionResult> GetLineSyncStatus(string lineId)
        {
            try
            {
                var status = await _syncMonitor.GetSyncStatusAsync(lineId);
                if (status == null)
                {
                    return NotFound(new { Error = $"No sync status found for line {lineId}" });
                }
                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sync status for line {LineId}", lineId);
                return StatusCode(500, new { Error = "Failed to retrieve sync status" });
            }
        }
    }
}