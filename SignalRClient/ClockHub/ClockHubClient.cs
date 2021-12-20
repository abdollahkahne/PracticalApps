using Microsoft.AspNetCore.SignalR.Client;

#nullable disable
namespace SignalRClient.Hubs
{
    public class ClockHubClient // we should use this class singleton to prevent creating it again and again and then handling the event with every instnces!
    {
        private readonly HubConnection _hubConnection;
        public ClockHubClient()
        {
            _hubConnection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5163/clock")
            .ConfigureLogging(options =>
            {

            })
            .WithAutomaticReconnect().Build();

            if (_hubConnection.State == HubConnectionState.Disconnected)
            {
                _hubConnection.StartAsync();
            }
            // Closed event fired after automatic reconnect and four sequential failure
            _hubConnection.Closed += async (err) =>
            {
                Console.WriteLine(err.Message);
                // wait between 0-5 second
                await Task.Delay(new Random().Next(1, 5) * 1000);
                await _hubConnection.StartAsync();

            };
            _hubConnection.On("ShowTime", (DateTime input) =>
            {
                Console.WriteLine("A Message from worker services sent to All clients: " + input);
            });
        }

        // public void Dispose()
        // {
        //     _hubConnection.DisposeAsync(); // In this case we should dispose the hub at the end since its event emitter is background services. But this make the connection disposed right after creation in case of Transient Service Lifetime!!
        //                                    // In case we have not separate client and server we can start and stop the connection on service worker start and stop
        // // another approach is to use this service singleton which is used here
        // }

        // Destructor
        ~ClockHubClient()
        {
            Console.WriteLine("Calling the destructor/finalize method here");
            _hubConnection.DisposeAsync();
        }

        public void Log()
        {
            Console.WriteLine("I invoked from service worker and respond here through the clockhub class lib");
        }

    }
}