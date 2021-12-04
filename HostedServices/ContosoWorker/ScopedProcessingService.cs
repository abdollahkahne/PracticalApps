using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ContosoWorker
{
    public interface IScopedProcessingService
    {
        Task DoStuf(CancellationToken stoppingToken);
    }
    public class ScopedProcessingService : IScopedProcessingService // scoped background task service contains the background task's logic (For example consider EF Core which should injected scoped)
    {
        private int _executionCount = 0;
        private readonly ILogger<ScopedProcessingService> _logger;

        public ScopedProcessingService(ILogger<ScopedProcessingService> logger)
        {
            _logger = logger;
        }

        public async Task DoStuf(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _executionCount++;
                _logger.LogInformation("Scoped Processing Service is working. Count: {count}", _executionCount);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}