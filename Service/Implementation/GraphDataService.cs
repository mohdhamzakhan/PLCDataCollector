using Microsoft.Extensions.Caching.Memory;
using PLCDataCollector.Model;
using PLCDataCollector.Service.Interfaces;

namespace PLCDataCollector.Service.Implementation
{
    public class GraphDataService : IGraphDataService
    {
        private readonly ILogger<GraphDataService> _logger;
        private readonly IConfigurationService _configService;
        private readonly IMemoryCache _cache;
        private static readonly Dictionary<string, List<GraphPoint>> _actualProductionCache = new();
        private static readonly Dictionary<string, List<GraphPoint>> _plannedProductionCache = new();

        public GraphDataService(
            ILogger<GraphDataService> logger,
            IConfigurationService configService,
            IMemoryCache cache)
        {
            _logger = logger;
            _configService = configService;
            _cache = cache;
        }

        public async Task<RealTimeGraphData> GetRealTimeGraphDataAsync(string lineId)
        {
            var currentShift = _configService.GetCurrentShift();
            var lineDetail = _configService.GetLineDetail(lineId);

            var cacheKey = $"graph_data_{lineId}_{DateTime.Now:yyyyMMdd}_{currentShift?.Name}";

            if (_cache.TryGetValue(cacheKey, out RealTimeGraphData cachedData))
            {
                return cachedData;
            }

            var graphData = new RealTimeGraphData
            {
                CurrentShift = currentShift?.Name ?? "Unknown",
                LastUpdated = DateTime.Now,
                ActualProduction = GetActualProductionPoints(lineId),
                PlannedProduction = GeneratePlannedProductionPoints(lineId, currentShift),
                ShiftBoundaries = GenerateShiftBoundaries(lineDetail)
            };

            // Cache for 30 seconds
            _cache.Set(cacheKey, graphData, TimeSpan.FromSeconds(30));

            return graphData;
        }

        public async Task<RealTimeGraphData> GetShiftGraphDataAsync(string lineId, DateTime date, string shift)
        {
            // Implementation for historical shift data
            return new RealTimeGraphData
            {
                CurrentShift = shift,
                LastUpdated = DateTime.Now,
                ActualProduction = new List<GraphPoint>(),
                PlannedProduction = new List<GraphPoint>()
            };
        }

        public async Task UpdateGraphDataAsync(string lineId, PLCData plcData)
        {
            if (!_actualProductionCache.ContainsKey(lineId))
            {
                _actualProductionCache[lineId] = new List<GraphPoint>();
            }

            var points = _actualProductionCache[lineId];

            // Add new point
            points.Add(new GraphPoint
            {
                Timestamp = plcData.Timestamp,
                Value = plcData.CurrentCount
            });

            // Keep only last 100 points (configurable)
            var maxPoints = _configService.GetAppSettings().GraphSettings?.MaxDataPoints ?? 100;
            if (points.Count > maxPoints)
            {
                points.RemoveAt(0);
            }

            _logger.LogDebug("Updated graph data for line {LineId}, points count: {Count}", lineId, points.Count);
        }

        private List<GraphPoint> GetActualProductionPoints(string lineId)
        {
            return _actualProductionCache.GetValueOrDefault(lineId, new List<GraphPoint>());
        }

        private List<GraphPoint> GeneratePlannedProductionPoints(string lineId, ShiftDetail currentShift)
        {
            if (currentShift == null) return new List<GraphPoint>();

            var points = new List<GraphPoint>();
            var shiftStart = DateTime.Today.Add(TimeSpan.Parse(currentShift.StartTime));
            var shiftEnd = DateTime.Today.Add(TimeSpan.Parse(currentShift.EndTime));

            // Handle overnight shifts
            if (shiftEnd < shiftStart)
            {
                shiftEnd = shiftEnd.AddDays(1);
            }

            var totalMinutes = (shiftEnd - shiftStart).TotalMinutes;
            var plannedRate = 60; // pieces per hour (configurable)

            for (int i = 0; i <= totalMinutes; i += 5) // Every 5 minutes
            {
                var timestamp = shiftStart.AddMinutes(i);
                if (timestamp <= DateTime.Now)
                {
                    var expectedCount = (int)(i / 60.0 * plannedRate);
                    points.Add(new GraphPoint
                    {
                        Timestamp = timestamp,
                        Value = expectedCount
                    });
                }
            }

            return points;
        }

        private List<ShiftBoundary> GenerateShiftBoundaries(LineDetail lineDetail)
        {
            var boundaries = new List<ShiftBoundary>();

            if (lineDetail?.ShiftConfiguration != null)
            {
                var shifts = new[]
                {
                    lineDetail.ShiftConfiguration.ShiftA,
                    lineDetail.ShiftConfiguration.ShiftB,
                    lineDetail.ShiftConfiguration.ShiftC
                };

                foreach (var shift in shifts)
                {
                    boundaries.Add(new ShiftBoundary
                    {
                        Timestamp = DateTime.Today.Add(TimeSpan.Parse(shift.StartTime)),
                        ShiftName = shift.Name,
                        Color = shift.Color
                    });
                }
            }

            return boundaries;
        }
    }
}
