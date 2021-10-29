using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace NorthwindCookieAuth.Diagnostics
{
    public class ExampleHealthCheckWithArgs:IHealthCheck
    {
        public string Url;

        public ExampleHealthCheckWithArgs(string url)
        {
            Url = url;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (Url.Contains("google")) {
                return Task.FromResult(new HealthCheckResult(HealthStatus.Healthy,"This is another way to generate a healthy HealthCheckResult"));
            } else {
                return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus,"The Health Status read from AddHealthCheck Registeration"));
            }
        }
    }
}