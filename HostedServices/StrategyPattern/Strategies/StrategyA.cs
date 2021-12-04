using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace StrategyPattern.Strategies
{
    public class StrategyA : IStrategy
    {
        private readonly ILogger<StrategyA> _logger;

        public StrategyA(ILogger<StrategyA> logger)
        {
            _logger = logger;
            _logger.LogInformation("Strategy A Created");
        }

        public async Task ExecuteAsync()
        {
            await Task.Run(() => _logger.LogInformation("Strategy A Executed"));
        }
    }
}