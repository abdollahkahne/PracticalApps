using Microsoft.AspNetCore.SignalR;

namespace SignalRClient.Hubs
{
    public class ClockHub : Hub
    {
        public async Task SendTimeToClients(DateTime dateTime)
        {
            await Clients.All.SendAsync("ShowTime", dateTime);
        }

    }
}