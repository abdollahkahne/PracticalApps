using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace NorthwindCookieAuth.Diagnostics
{
    public class ExampleHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var healthyResult = false;
            if (healthyResult)
            {
                return Task.FromResult(HealthCheckResult.Healthy("A Healthy Result Example"));
            }
            else
            {
                // return Task.FromResult(HealthCheckResult.Unhealthy("An unhealthy HealthCheckResult Example"));
                return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, "An unhealthy HealthCheckResult using its Constructore"));
            }
        }
    }
}