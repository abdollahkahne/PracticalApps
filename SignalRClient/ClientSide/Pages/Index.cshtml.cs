using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SignalRClient.Hubs;

namespace ClientSide.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly HubClient _hubClient;
    private readonly ClockHubClient _clockHubClient;

    public IndexModel(ILogger<IndexModel> logger, HubClient hubClient, ClockHubClient clockHubClient)
    {
        _logger = logger;
        _hubClient = hubClient;
        _clockHubClient = clockHubClient;
    }

    public async Task OnGet(CancellationToken cancellationToken)
    {
        // await _hubClient.SendMessage(new ServerSide.Hubs.MessageInput { Messsage = "Called From .Net Client" });

        // await _hubClient.GetCounterChannelStream(new ServerSide.Hubs.CounterInput { Count = 300, Delay = 1000 }, cancellationToken);
        // await _hubClient.SendChannelStream(new ServerSide.Hubs.CounterInput { Count = 5, Delay = 5000 }, cancellationToken);

        // await _hubClient.GetCounterStream(new ServerSide.Hubs.CounterInput { Count = 10, Delay = 1200 });
        // await _hubClient.SendChannelStream(new ServerSide.Hubs.CounterInput { Count = 10, Delay = 2000 }, cancellationToken);
        _clockHubClient.Log();
    }
    public async Task<IActionResult> OnPostAsync()
    {
        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;
        var rng = new Random();
        if (rng.NextSingle() > 0.5)
        {
            var cancelInSecond = rng.Next(1, 10);
            cts.CancelAfter(cancelInSecond * 1000);
            Console.WriteLine($"Cacellation Token Cancelled at {cancelInSecond}");
        }
        // await _hubClient.SendChannelStream(new ServerSide.Hubs.CounterInput { Count = 10, Delay = 1200 }, cancellationToken);
        await _hubClient.SendAsyncStream(new ServerSide.Hubs.CounterInput { Count = 10, Delay = 1000 }, cancellationToken);
        return Page();
    }
}
