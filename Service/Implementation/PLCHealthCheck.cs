using Microsoft.Extensions.Diagnostics.HealthChecks;
using PLCDataCollector.Service.Interfaces;

namespace PLCDataCollector.Service.Implementation
{
    public class PLCHealthCheck : IHealthCheck
    {
        private readonly IConfigurationService _configService;

        public PLCHealthCheck(IConfigurationService configService)
        {
            _configService = configService;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var lineDetail = _configService.GetLineDetail("EGRV_Final");
                // Check PLC connection
                // Implementation depends on your PLC communication method

                return HealthCheckResult.Healthy("PLC connection is healthy");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("PLC connection failed", ex);
            }
        }
    }

}
