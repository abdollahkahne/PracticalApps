using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace StrategyPattern.Strategies
{
    public class StrategyB : IStrategy
    {
        private readonly ILogger<StrategyB> _logger;

        public StrategyB(ILogger<StrategyB> logger)
        {
            _logger = logger;
            _logger.LogInformation("Strategy B Created");
        }

        public async Task ExecuteAsync()
        {
            await Task.Run(() => _logger.LogInformation("Strategy B Executed"));
        }
    }
}