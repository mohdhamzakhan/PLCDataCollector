using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PLCDataCollector.Service;
using PLCDataCollector.Service.Interfaces;

namespace PLCDataCollector.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IProductionService _productionService;
        private readonly IConfigurationService _configService;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(
            IProductionService productionService,
            IConfigurationService configService,
            ILogger<ReportsController> logger)
        {
            _productionService = productionService;
            _configService = configService;
            _logger = logger;
        }

        /// <summary>
        /// Get production summary for a specific date
        /// </summary>
        /// <param name="lineId">Production line identifier</param>
        /// <param name="date">Date for the report</param>
        /// <returns>Daily production summary</returns>
        [HttpGet("daily-summary/{lineId}")]
        public async Task<ActionResult> GetDailySummary(string lineId, [FromQuery] DateTime date)
        {
            try
            {
                var lineDetail = _configService.GetLineDetail(lineId);
                if (lineDetail == null)
                {
                    return BadRequest(new { error = "Invalid line ID" });
                }

                var shifts = new[] { "ShiftA", "ShiftB", "ShiftC" };
                var shiftSummaries = new List<object>();

                foreach (var shift in shifts)
                {
                    var shiftData = await _productionService.GetShiftProductionAsync(lineId, date, shift);

                    var summary = new
                    {
                        Shift = shift,
                        TotalProduction = shiftData.Sum(d => d.ActualCount),
                        PlannedProduction = shiftData.Sum(d => d.PlannedCount),
                        AverageEfficiency = shiftData.Any() ? shiftData.Average(d => d.Efficiency) : 0,
                        DataPoints = shiftData.Count
                    };
                    shiftSummaries.Add(summary);
                }

                var dailySummary = new
                {
                    Date = date.ToString("yyyy-MM-dd"),
                    LineId = lineId,
                    LineName = lineDetail.LineName,
                    TotalProduction = shiftSummaries.Sum(s => ((dynamic)s).TotalProduction),
                    TotalPlanned = shiftSummaries.Sum(s => ((dynamic)s).PlannedProduction),
                    OverallEfficiency = shiftSummaries.Any() ?
                        shiftSummaries.Average(s => ((dynamic)s).AverageEfficiency) : 0,
                    ShiftSummaries = shiftSummaries
                };

                return Ok(dailySummary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get daily summary for line {LineId} on {Date}", lineId, date);
                return StatusCode(500, new { error = "Failed to generate daily summary" });
            }
        }

        /// <summary>
        /// Get efficiency report for a date range
        /// </summary>
        /// <param name="lineId">Production line identifier</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>Efficiency trend report</returns>
        [HttpGet("efficiency-report/{lineId}")]
        public async Task<ActionResult> GetEfficiencyReport(
            string lineId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                if (endDate < startDate)
                {
                    return BadRequest(new { error = "End date must be after start date" });
                }

                var reportData = new List<object>();
                var currentDate = startDate;

                while (currentDate <= endDate)
                {
                    var shifts = new[] { "ShiftA", "ShiftB", "ShiftC" };
                    var dayEfficiency = new List<double>();

                    foreach (var shift in shifts)
                    {
                        var shiftData = await _productionService.GetShiftProductionAsync(lineId, currentDate, shift);
                        if (shiftData.Any())
                        {
                            dayEfficiency.Add(shiftData.Average(d => d.Efficiency));
                        }
                    }

                    reportData.Add(new
                    {
                        Date = currentDate.ToString("yyyy-MM-dd"),
                        AverageEfficiency = dayEfficiency.Any() ? dayEfficiency.Average() : 0,
                        ShiftCount = dayEfficiency.Count
                    });

                    currentDate = currentDate.AddDays(1);
                }

                var report = new
                {
                    LineId = lineId,
                    StartDate = startDate.ToString("yyyy-MM-dd"),
                    EndDate = endDate.ToString("yyyy-MM-dd"),
                    OverallEfficiency = reportData.Any() ?
                        reportData.Average(d => ((dynamic)d).AverageEfficiency) : 0,
                    DailyData = reportData
                };

                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get efficiency report for line {LineId}", lineId);
                return StatusCode(500, new { error = "Failed to generate efficiency report" });
            }
        }
    }
}
