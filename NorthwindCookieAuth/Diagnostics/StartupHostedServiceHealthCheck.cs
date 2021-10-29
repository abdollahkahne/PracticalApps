using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace NorthwindCookieAuth.Diagnostics
{
    public class StartupHostedServiceHealthCheck : IHealthCheck
    {
        // The volatile keyword indicates that a field might be modified by multiple threads that are executing at the same time.
        private volatile bool _startupTaskCompleted = false;
        public bool StartupTaskCompleted
        {
            get => _startupTaskCompleted;
            set => _startupTaskCompleted = value;
        }
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (StartupTaskCompleted)
            {
                return Task.FromResult(HealthCheckResult.Healthy("The startup task is finished."));
            }
            else
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("The startup task is still running."));
            }
        }
    }
}