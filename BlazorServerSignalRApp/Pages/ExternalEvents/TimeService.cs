using System.Timers;

namespace BlazorServerSignalRApp.Pages.ExternalEvents
{
    public class TimeService : IDisposable
    {
        private int elapsedCount;
        private readonly NotifierService _notifierService;
        private readonly ILogger<TimeService> _logger;
        private System.Timers.Timer? timer; // with Timer we can generate an event after defined interval as recurring events! 
        public TimeService(NotifierService notifierService, ILogger<TimeService> logger)
        {
            _notifierService = notifierService;
            _logger = logger;
        }

        public void Start()
        {
            if (timer == null)
            {
                timer = new();
                timer.AutoReset = true;
                timer.Interval = 1000;
                timer.Enabled = true;
                timer.Elapsed += handleTimer;
                _logger.LogInformation("Timer Service Started!");
            }
        }
        private async void handleTimer(object? source, ElapsedEventArgs e)
        {
            elapsedCount += 1;
            await _notifierService.OnNotify("elapsedCount", elapsedCount); // We call event raiser method 
            _logger.LogInformation($"ElapsedCount:{elapsedCount}");
        }
        public void Dispose()
        {
            timer?.Dispose();
        }
    }
}

// Time Service is a simple service which uses timer to set interval an event
// Here the event which used on set interval is Notifier which defined in Notifier Service