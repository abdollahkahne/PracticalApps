using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NorthwindCookieAuth.Diagnostics;

namespace NorthwindCookieAuth.HostedServices
{
    public class StartupHostedService : IHostedService, IDisposable
    {
        private readonly int _delaySeconds = 15;
        private readonly ILogger<StartupHostedService> _logger;
        private readonly StartupHostedServiceHealthCheck _startupHostedServiceHealthCheck;
        public StartupHostedService(ILogger<StartupHostedService> logger, StartupHostedServiceHealthCheck startupHostedServiceHealthCheck)
        {
            _logger = logger;
            _startupHostedServiceHealthCheck = startupHostedServiceHealthCheck;
        }
        public void Dispose()
        {

        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Startup Background Service is starting.");

            // Simulate the effect of a long-running startup task.
            Task.Run(async () =>
            {
                await Task.Delay(_delaySeconds * 1000);
                _startupHostedServiceHealthCheck.StartupTaskCompleted = true;
                _logger.LogInformation("Startup Background Service has started.");
            });
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Startup Background Service is stopping.");

            return Task.CompletedTask;
        }
    }
}