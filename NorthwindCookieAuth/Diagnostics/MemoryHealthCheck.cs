using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace NorthwindCookieAuth.Diagnostics
{
    public static class HealthCheckExtensions
    {
        public static IHealthChecksBuilder AddMemoryHealthCheck(this IHealthChecksBuilder healthChecksBuilder,
        string name, HealthStatus failureStatus = HealthStatus.Degraded,
        IEnumerable<string> tags = null,
        long? tresholdInBytes = null)
        {
            healthChecksBuilder.AddCheck<MemoryHealthCheck>(name, failureStatus, tags);
            if (tresholdInBytes.HasValue)
            {
                // Configure named options to pass the threshold into the check. (In case we have different named options)
                healthChecksBuilder.Services.Configure<MemoryCheckOptions>(name, options => options.Treshhold = tresholdInBytes.Value);
            }
            return healthChecksBuilder;
        }
    }
    public class MemoryHealthCheck : IHealthCheck
    {
        private readonly IOptionsMonitor<MemoryCheckOptions> _options;
        public string Name => "memory_check";

        public MemoryHealthCheck(IOptionsMonitor<MemoryCheckOptions> options)
        {
            _options = options;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var options = _options.Get(context.Registration.Name);
            var allocated = GC.GetTotalMemory(forceFullCollection: false); // true to indicate that this method can wait for garbage collection to occur before returning; otherwise, false
            var data = new Dictionary<string, object> {
                {"AllocatedBytes",allocated},
                {"Gen0Collections",GC.CollectionCount(0)},
                {"Gen1Collections",GC.CollectionCount(1)},
                {"Gen2Collections",GC.CollectionCount(2)}
            };
            var status = (allocated <= options.Treshhold) ? HealthStatus.Healthy : context.Registration.FailureStatus;
            HealthCheckResult result = new HealthCheckResult(status, $"Reports Degraded Status if allocated byte >= {options.Treshhold}", null, data);
            return Task.FromResult(result);
        }
    }
    public class MemoryCheckOptions
    {
        // Convert 1GB to bytes as default value for treshold
        public long Treshhold { get; set; } = 1024L * 1024L * 1024L;
    }
}