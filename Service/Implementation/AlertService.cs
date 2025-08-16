using Microsoft.Extensions.Caching.Memory;
using PLCDataCollector.Model.Classes;

namespace PLCDataCollector.Service.Implementation
{
    public interface IAlertService
    {
        Task<List<ProductionAlert>> GetActiveAlertsAsync(string lineId);
        Task AddAlertAsync(ProductionAlert alert);
        Task ResolveAlertAsync(string alertId);
        Task<List<ProductionAlert>> GetAlertHistoryAsync(string lineId, DateTime fromDate, DateTime toDate);
    }

    public class AlertService : IAlertService
    {
        private readonly ILogger<AlertService> _logger;
        private readonly IMemoryCache _cache;
        private static readonly Dictionary<string, List<ProductionAlert>> _alertCache = new();

        public AlertService(ILogger<AlertService> logger, IMemoryCache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        public async Task<List<ProductionAlert>> GetActiveAlertsAsync(string lineId)
        {
            if (_alertCache.TryGetValue(lineId, out var alerts))
            {
                return alerts.Where(a => a.Timestamp > DateTime.Now.AddHours(-1)).ToList(); // Active for 1 hour
            }
            return new List<ProductionAlert>();
        }

        public async Task AddAlertAsync(ProductionAlert alert)
        {
            var lineId = "default"; // You might want to add LineId to ProductionAlert model

            if (!_alertCache.ContainsKey(lineId))
            {
                _alertCache[lineId] = new List<ProductionAlert>();
            }

            _alertCache[lineId].Add(alert);

            // Keep only last 50 alerts per line
            if (_alertCache[lineId].Count > 50)
            {
                _alertCache[lineId].RemoveAt(0);
            }

            _logger.LogInformation("Alert added: {Type} - {Message}", alert.Type, alert.Message);
        }

        public async Task ResolveAlertAsync(string alertId)
        {
            // Implementation for resolving specific alerts
            _logger.LogInformation("Alert resolved: {AlertId}", alertId);
        }

        public async Task<List<ProductionAlert>> GetAlertHistoryAsync(string lineId, DateTime fromDate, DateTime toDate)
        {
            if (_alertCache.TryGetValue(lineId, out var alerts))
            {
                return alerts.Where(a => a.Timestamp >= fromDate && a.Timestamp <= toDate).ToList();
            }
            return new List<ProductionAlert>();
        }
    }
}
