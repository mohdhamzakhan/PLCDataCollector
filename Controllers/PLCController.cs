using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PLCDataCollector.Service;
using PLCDataCollector.Service.Interfaces;

namespace PLCDataCollector.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PLCController : ControllerBase
    {
        private readonly IPLCService _plcService;
        private readonly ILogger<PLCController> _logger;

        public PLCController(IPLCService plcService, ILogger<PLCController> logger)
        {
            _plcService = plcService;
            _logger = logger;
        }

        /// <summary>
        /// Test PLC connection for a specific line
        /// </summary>
        /// <param name="lineId">Production line identifier</param>
        /// <returns>Connection status</returns>
        [HttpGet("test-connection/{lineId}")]
        public async Task<ActionResult> TestPLCConnection(string lineId)
        {
            try
            {
                var isConnected = await _plcService.TestConnectionAsync(lineId);
                return Ok(new
                {
                    lineId,
                    connected = isConnected,
                    timestamp = DateTime.Now,
                    message = isConnected ? "Connection successful" : "Connection failed"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to test PLC connection for line {LineId}", lineId);
                return StatusCode(500, new { error = "Failed to test PLC connection" });
            }
        }

        /// <summary>
        /// Get raw PLC data for debugging purposes
        /// </summary>
        /// <param name="lineId">Production line identifier</param>
        /// <returns>Raw PLC data</returns>
        [HttpGet("raw-data/{lineId}")]
        public async Task<ActionResult> GetRawPLCData(string lineId)
        {
            try
            {
                var rawData = await _plcService.ReadRawDataAsync(lineId);
                return Ok(new { lineId, data = rawData, timestamp = DateTime.Now });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get raw PLC data for line {LineId}", lineId);
                return StatusCode(500, new { error = "Failed to retrieve raw PLC data" });
            }
        }

        /// <summary>
        /// Get processed PLC data
        /// </summary>
        /// <param name="lineId">Production line identifier</param>
        /// <returns>Processed PLC data</returns>
        [HttpGet("data/{lineId}")]
        public async Task<ActionResult> GetPLCData(string lineId)
        {
            try
            {
                var plcData = await _plcService.ReadPLCDataAsync(lineId);
                return Ok(plcData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get PLC data for line {LineId}", lineId);
                return StatusCode(500, new { error = "Failed to retrieve PLC data" });
            }
        }
    }
}
