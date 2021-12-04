using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ContosoWorker
{
    public class MyServiceHostedService : BackgroundService // StartAsync and StopAsync implemented in Background Service So we can dont use them
    {
        // private readonly ScopedProcessingService _scoped;
        private readonly ILogger<MyServiceHostedService> _logger;
        private readonly IServiceProvider _serviceProvider; // Here we can use IServiceScopeFactory to inject which has same implementation

        public MyServiceHostedService(IServiceProvider serviceProvider, ILogger<MyServiceHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("My Hosted Service is working");
            await DoWork(stoppingToken);

        }
        private async Task DoWork(CancellationToken stoppingToken)
        {

            using (var scope = _serviceProvider.CreateScope())
            {
                var processingService = scope.ServiceProvider.GetRequiredService<IScopedProcessingService>();
                await processingService.DoStuf(stoppingToken);
            }
        }
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("MyServiceHostedService is stopping");
            await base.StopAsync(cancellationToken);
        }
    }
}