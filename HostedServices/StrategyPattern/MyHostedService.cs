using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace StrategyPattern
{
    public class MyHostedService : BackgroundService
    {
        private readonly ILogger<MyHostedService> _logger;
        // private readonly IServiceProvider _serviceProvider; // This does not need Microsoft.Extensions.DependencyInjection but the following approach needs it
        private readonly IServiceScopeFactory _scopeFactory;// This needs another package installation which is Microsoft.Extensions.DependencyInjection other than Microsoft.Extensions.Hosting which added by default to worker template

        public MyHostedService(ILogger<MyHostedService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) // Altough This method may returned but the Hosted service never stoped unless we press ctrl+C or unwanted failure!
        {
            _logger.LogInformation("My Hosted Service is Running");
            var type = DriverQueue.TryDequeue();
            while (type != null && !stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    IStrategy cmd = scope.ServiceProvider.GetRequiredService(type) as IStrategy;
                    if (cmd != null)
                    {
                        await cmd.ExecuteAsync();
                    }
                    await Task.Delay(3000, stoppingToken);
                }

                type = DriverQueue.TryDequeue();
            }
        }
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("My Hosted Service Stopped");
            return base.StopAsync(cancellationToken);
        }
    }
}