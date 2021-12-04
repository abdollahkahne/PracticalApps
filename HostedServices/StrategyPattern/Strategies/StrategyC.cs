using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace StrategyPattern.Strategies
{
    public class StrategyC : IStrategy
    {
        private readonly ILogger<StrategyC> _logger;

        public StrategyC(ILogger<StrategyC> logger)
        {
            _logger = logger;
            _logger.LogInformation("Strategy C Created");
        }

        public async Task ExecuteAsync()
        {
            await Task.Run(() => _logger.LogInformation("Strategy C Executed"));
        }
    }
}