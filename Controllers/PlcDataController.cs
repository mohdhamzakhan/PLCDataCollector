using Microsoft.AspNetCore.Mvc;
using PLCDataCollector.Model.Exceptions;
using PLCDataCollector.Model.Validation;
using PLCDataCollector.Service.Interfaces;
using FluentValidation;

namespace PLCDataCollector.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlcDataController : ControllerBase
    {
        private readonly IPlcDataService _plcDataService;
        private readonly ILogger<PlcDataController> _logger;
        private readonly PlcDataValidator _validator;

        public PlcDataController(
            IPlcDataService plcDataService,
            ILogger<PlcDataController> logger)
        {
            _plcDataService = plcDataService;
            _logger = logger;
            _validator = new PlcDataValidator();
        }

        [HttpPost("{lineId}")]
        public async Task<IActionResult> SavePlcData(string lineId, [FromBody] Dictionary<string, object> data)
        {
            try
            {
                // Validate input data
                var validationResult = await _validator.ValidateAsync(data);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new
                    {
                        Errors = validationResult.Errors.Select(e => e.ErrorMessage)
                    });
                }

                await _plcDataService.SavePlcDataAsync(lineId, data);
                return Ok(new { Message = "Data saved successfully" });
            }
            catch (DatabaseException ex)
            {
                _logger.LogError(ex, "Database error while saving PLC data for line {LineId}", lineId);
                return StatusCode(500, new
                {
                    Error = "Database operation failed",
                    Details = ex.Message,
                    Operation = ex.Operation
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while saving PLC data for line {LineId}", lineId);
                return StatusCode(500, new { Error = "An unexpected error occurred" });
            }
        }

        [HttpGet("{lineId}/unsynced")]
        public async Task<IActionResult> GetUnsyncedData(string lineId)
        {
            try
            {
                var data = await _plcDataService.GetUnsyncedDataAsync(lineId);
                return Ok(data);
            }
            catch (DatabaseException ex)
            {
                _logger.LogError(ex, "Database error while getting unsynced data for line {LineId}", lineId);
                return StatusCode(500, new
                {
                    Error = "Database operation failed",
                    Details = ex.Message,
                    Operation = ex.Operation
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while getting unsynced data for line {LineId}", lineId);
                return StatusCode(500, new { Error = "An unexpected error occurred" });
            }
        }

        [HttpPut("{lineId}/sync/{recordId}")]
        public async Task<IActionResult> UpdateSyncStatus(string lineId, int recordId)
        {
            try
            {
                var success = await _plcDataService.UpdateSyncStatusAsync(lineId, recordId);
                if (!success)
                {
                    return NotFound(new { Error = "Record not found or already synced" });
                }
                return Ok(new { Message = "Sync status updated successfully" });
            }
            catch (DatabaseException ex)
            {
                _logger.LogError(ex, "Database error while updating sync status for line {LineId}, record {RecordId}", lineId, recordId);
                return StatusCode(500, new
                {
                    Error = "Database operation failed",
                    Details = ex.Message,
                    Operation = ex.Operation
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating sync status for line {LineId}, record {RecordId}", lineId, recordId);
                return StatusCode(500, new { Error = "An unexpected error occurred" });
            }
        }
    }
}