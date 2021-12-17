using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using SignalRClient.Hubs;

#nullable disable
namespace WorkerSignalUser;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private IHubContext<ClockHub> _clockHub;
    private HubConnection _hubConnection;
    private readonly IServiceProvider _sp;

    public Worker(ILogger<Worker> logger, IServiceProvider sp)
    {
        _logger = logger;
        _sp = sp;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using (var scope = _sp.CreateScope())
        {
            _clockHub = scope.ServiceProvider.GetRequiredService<IHubContext<ClockHub>>();
        }
        await DoStuff(stoppingToken);

    }

    private async Task DoStuff(CancellationToken stoppingToken)
    {
        await Task.Delay(5000, stoppingToken);
        StartHubConnection(stoppingToken); // Start of connection should be at execution an not at the start of background service. Altough execution itself triggered from start. so delay it for some time
        // since at service start we have not the url of chathub available
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await _clockHub.Clients.All.SendAsync("ShowTime", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
        }
    }


    // we can build and start connection in background services too
    private void StartHubConnection(CancellationToken cancellationToken)
    {

        _hubConnection.On("ShowTime", (DateTime input) =>
        {
            Console.WriteLine("Inside Worker: A Message sent to All clients: " + input);
        });

        // Closed event fired after automatic reconnect and four sequential failure
        _hubConnection.Closed += async (err) =>
        {
            _logger.LogError(err.Message);
            // wait between 0-5 second
            await Task.Delay(new Random().Next(1, 5) * 1000);
            await _hubConnection.StartAsync(cancellationToken);

        };

        // Start Connection after setting the events handlers
        _hubConnection.StartAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _hubConnection.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }

    // This may happen before creating the Hub itself in server so start it in other steps like 
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5163/clock")
            .WithAutomaticReconnect().Build();
        return base.StartAsync(cancellationToken);
    }
}
