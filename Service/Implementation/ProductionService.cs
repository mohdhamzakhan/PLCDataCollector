using PLCDataCollector.Model;
using PLCDataCollector.Service.Interfaces;

namespace PLCDataCollector.Service.Implementation
{
    public class ProductionService : IProductionService
    {
        private readonly IConfigurationService _configService;
        private readonly IPLCService _plcService;
        private readonly IGraphDataService _graphDataService;
        private readonly ILogger<ProductionService> _logger;
        private readonly string _connectionString;

        public ProductionService(
            IConfigurationService configService,
            IPLCService plcService,
            IGraphDataService graphDataService,
            ILogger<ProductionService> logger)
        {
            _configService = configService;
            _plcService = plcService;
            _graphDataService = graphDataService;
            _logger = logger;
            _connectionString = _configService.GetAppSettings().ConnectionStrings.Production;
        }

        public async Task<ProductionData> GetRealTimeProductionAsync(string lineId)
        {
            try
            {
                var plcData = await _plcService.ReadPLCDataAsync(lineId);
                var currentShift = _configService.GetCurrentShift();

                var productionData = new ProductionData
                {
                    LineId = lineId,
                    Timestamp = plcData.Timestamp,
                    ActualCount = plcData.CurrentCount,
                    PlannedCount = CalculatePlannedCount(currentShift),
                    CycleTime = plcData.CycleTime,
                    PartNumber = plcData.PartNumber,
                    ShiftName = currentShift?.Name ?? "Unknown",
                    Status = plcData.IsRunning ? ProductionStatus.Running : ProductionStatus.Idle
                };

                productionData.Efficiency = CalculateEfficiency(productionData);

                // Update graph data
                await _graphDataService.UpdateGraphDataAsync(lineId, plcData);

                return productionData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get real-time production data for line {LineId}", lineId);
                throw;
            }
        }

        public async Task<List<ProductionData>> GetShiftProductionAsync(string lineId, DateTime date, string shift)
        {
            // Implementation for historical shift data retrieval
            // This would typically query your database
            return new List<ProductionData>();
        }

        public async Task<ShiftStatus> GetCurrentShiftStatusAsync(string lineId)
        {
            var currentShift = _configService.GetCurrentShift();
            var productionData = await GetRealTimeProductionAsync(lineId);

            if (currentShift == null)
            {
                return new ShiftStatus
                {
                    Status = ProductionStatus.Error,
                    Alerts = new List<ProductionAlert>
                    {
                        new() { Type = "Configuration", Message = "No shift configuration found", Severity = AlertSeverity.Critical, Timestamp = DateTime.Now }
                    }
                };
            }

            var shiftStartTime = DateTime.Today.Add(TimeSpan.Parse(currentShift.StartTime));
            var shiftEndTime = DateTime.Today.Add(TimeSpan.Parse(currentShift.EndTime));

            if (shiftEndTime < shiftStartTime) // Overnight shift
            {
                shiftEndTime = shiftEndTime.AddDays(1);
            }

            var timeRemaining = shiftEndTime - DateTime.Now;
            var plannedProduction = CalculatePlannedCount(currentShift);

            var status = new ShiftStatus
            {
                CurrentShift = currentShift.Name,
                ShiftStartTime = shiftStartTime,
                ShiftEndTime = shiftEndTime,
                TimeRemaining = timeRemaining,
                ActualProduction = productionData.ActualCount,
                PlannedProduction = plannedProduction,
                EfficiencyPercentage = productionData.Efficiency,
                Status = productionData.Status,
                Alerts = GenerateAlerts(productionData, plannedProduction)
            };

            return status;
        }

        public async Task SaveProductionPlanAsync(ProductionPlan plan)
        {
            // Implementation for saving production plans
            // This would typically save to your database
            _logger.LogInformation("Production plan saved for line {LineId}, part {PartNumber}", plan.LineId, plan.PartNumber);
        }

        public async Task CollectRealTimeDataAsync()
        {
            var appSettings = _configService.GetAppSettings();

            foreach (var lineDetail in appSettings.LineDetails)
            {
                try
                {
                    var productionData = await GetRealTimeProductionAsync(lineDetail.Key);
                    // Here you would typically save to database
                    _logger.LogDebug("Collected data for line {LineId}: {Count} pieces", lineDetail.Key, productionData.ActualCount);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to collect data for line {LineId}", lineDetail.Key);
                }
            }
        }

        private int CalculatePlannedCount(ShiftDetail currentShift)
        {
            if (currentShift == null) return 0;

            var shiftStart = TimeSpan.Parse(currentShift.StartTime);
            var now = DateTime.Now.TimeOfDay;

            // Calculate minutes elapsed in shift
            var minutesElapsed = (now - shiftStart).TotalMinutes;
            if (minutesElapsed < 0) minutesElapsed += 24 * 60; // Handle overnight shifts

            // Assume 60 pieces per hour as default rate
            return (int)(minutesElapsed / 60.0 * 60);
        }

        private double CalculateEfficiency(ProductionData data)
        {
            if (data.PlannedCount == 0) return 0;
            return Math.Round((double)data.ActualCount / data.PlannedCount * 100, 2);
        }

        private List<ProductionAlert> GenerateAlerts(ProductionData data, int plannedCount)
        {
            var alerts = new List<ProductionAlert>();
            var settings = _configService.GetAppSettings().RealTimeSettings;

            if (settings?.AlertThresholds != null)
            {
                var efficiency = data.Efficiency;

                if (efficiency < 100 - settings.AlertThresholds.BehindSchedule)
                {
                    alerts.Add(new ProductionAlert
                    {
                        Type = "BehindSchedule",
                        Message = $"Production is {100 - efficiency:F1}% behind schedule",
                        Severity = efficiency < 85 ? AlertSeverity.Critical : AlertSeverity.Warning,
                        Timestamp = DateTime.Now
                    });
                }
                else if (efficiency > 100 + settings.AlertThresholds.AheadSchedule)
                {
                    alerts.Add(new ProductionAlert
                    {
                        Type = "AheadSchedule",
                        Message = $"Production is {efficiency - 100:F1}% ahead of schedule",
                        Severity = AlertSeverity.Info,
                        Timestamp = DateTime.Now
                    });
                }
            }

            if (data.Status == ProductionStatus.Idle)
            {
                alerts.Add(new ProductionAlert
                {
                    Type = "Idle",
                    Message = "Production line is currently idle",
                    Severity = AlertSeverity.Warning,
                    Timestamp = DateTime.Now
                });
            }

            return alerts;
        }
    }
}
