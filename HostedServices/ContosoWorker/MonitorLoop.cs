using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ContosoWorker
{
    public class MonitorLoop
    {
        private readonly IBackgroundTaskQueue _taskQueue; // Consider that only add to api for enquing and dequing. The queue itself is a private Channel Type property in it.
        private ILogger<MonitorLoop> _logger;
        private readonly CancellationToken _stoppingToken;

        public MonitorLoop(IBackgroundTaskQueue taskQueue, ILogger<MonitorLoop> logger, IHostApplicationLifetime applicationLifetime)
        {
            _taskQueue = taskQueue;
            _logger = logger;
            _stoppingToken = applicationLifetime.ApplicationStopping;
        }

        public void StartMonitorLoop()
        {
            _logger.LogInformation("MonitorAsync Loop is starting.");
            Task.Run(async () => await monitorAsync()); // Why we use run and not call it normally?
            // Since we want to do the task by another thread we use Task.Run method
            // Why we use the function that call our function: since we want to it from start to end using one thread here and do not switch every press of w! And I think it is not necessary
        }

        private async ValueTask monitorAsync()
        {
            while (!_stoppingToken.IsCancellationRequested)
            {
                var keyInfo = Console.ReadKey();
                if (keyInfo.Key == ConsoleKey.W)
                {
                    // await buildWorkItem(_stoppingToken);// We should not run it here! Only add it to queue stupid
                    await _taskQueue.QueueBackgroundWorkItemAsync(buildWorkItem);
                }
            }
        }
        private async ValueTask buildWorkItem(CancellationToken stoppingToken)
        {
            // Here we simulate a work item (it does not provided by user here, just press W to this method act as a new work item)
            int delayLoop = 0;
            var guid = Guid.NewGuid().ToString();
            _logger.LogInformation("Queued Background Task with Guid {guid} is starting", guid);
            while (!stoppingToken.IsCancellationRequested && delayLoop < 3)
            {
                try
                {
                    await Task.Delay(5000, stoppingToken);
                }
                catch (OperationCanceledException)
                {

                    // throw; // Do not throw anything and lets the Background Method do Stop Async Method
                }
                delayLoop++;
                _logger.LogInformation("Queued Background Task with Guid {guid} is passing step {step}/3", guid, delayLoop);
            }
            if (delayLoop == 3)
            {
                _logger.LogInformation("Queued Background Task {guid} is complete.", guid);
            }
            else
            {
                _logger.LogInformation("Queued Background Task {guid} was cancelled.", guid);
            }
        }
    }
}