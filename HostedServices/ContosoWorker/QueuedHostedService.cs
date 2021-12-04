using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ContosoWorker
{
    public class QueuedHostedService : BackgroundService
    {
        private readonly ILogger<QueuedHostedService> _logger;
        public IBackgroundTaskQueue TaskQueue { get; }// since the queue may need to read from instances of this class better to define it Public and set a getter for it (Property)

        public QueuedHostedService(ILogger<QueuedHostedService> logger, IBackgroundTaskQueue queue)
        {
            _logger = logger;
            TaskQueue = queue;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Queued Hosted Service is stopping.");
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"Queued Hosted Service is Running {Environment.NewLine}" +
            $"{Environment.NewLine} Press W to add a work item to the {Environment.NewLine}" +
            $"Background Queue {Environment.NewLine}"
            );
            await backgroundProcessing(stoppingToken);

        }
        private async Task backgroundProcessing(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // get an item from queue
                var workItem = await TaskQueue.DequeueAsync(stoppingToken);

                // use try-catch to catch exceptions here
                try
                {
                    await workItem(stoppingToken); // work item is a function delegate so we can invoke it or use it
                }
                catch (System.Exception ex)
                {

                    _logger.LogError(ex, "Error occured executing Work Item: {workItem}", nameof(workItem));
                }
            }

        }
    }
}