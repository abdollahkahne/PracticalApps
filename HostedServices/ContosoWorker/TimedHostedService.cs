using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ContosoWorker
{
    public class TimedHostedService : IHostedService, IDisposable
    {
        private int _executionCount = 0;
        private readonly ILogger<TimedHostedService> _logger;
        private Timer _timer = null;

        public TimedHostedService(ILogger<TimedHostedService> logger)
        {
            _logger = logger;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Hosted Service Running");
            // The Timer doesn't wait for previous executions of DoWork to finish, so the approach shown might not be suitable for every scenario.
            _timer = new Timer(doSomething, null, TimeSpan.Zero, TimeSpan.FromSeconds(5)); // setup an interval that start at 0 and repeat every 5 seconds
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Hosted Service is stopping");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        private void doSomething(Object state)
        {
            var count = Interlocked.Increment(ref _executionCount);// . Interlocked.Increment is used to increment the execution counter as an atomic operation, which ensures that multiple threads don't update executionCount concurrently.
            _logger.LogInformation("Timed Hosting Service is Counting. Count: {count}", count);
        }
    }
}