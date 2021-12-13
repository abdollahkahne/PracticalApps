using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using ServerSide.Hubs;

# nullable disable

namespace ClientSide
{
    public class HubClient
    {
        private readonly HubConnection _hubConnection;
        public HubClient()
        {
            _hubConnection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5163/chat", option =>
            {
                option.UseDefaultCredentials = true; // this make system to use the windows authentication
                // Windows authentication is supported in Internet Explorer and Microsoft Edge, but not in all browsers.
                // For example, in Chrome and Safari, attempting to use Windows authentication and WebSockets fails.
                // When Windows authentication fails, the client attempts to fall back to other transports which might work.
                option.Headers[Microsoft.Net.Http.Headers.HeaderNames.Authorization] = "SignalR";
            }).WithAutomaticReconnect().Build();
            // .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30) }) yields the default behavior

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
            _hubConnection.On(ChatEvents.Message, (MessageData data) =>
            {
                Console.WriteLine("A Message sent to All clients:" + data.Username + ": " + data.Message);
            });
        }
        public async Task SendMessage(MessageInput input)
        {
            await _hubConnection.InvokeAsync(ServerChatEvents.SendMessage, input);
        }
    }
    public class ServerChatEvents
    {
        public const string SendMessage = "SendMessage";
    }
    public class MessageData
    {
        public string Message { get; set; }
        public string Username { get; set; }
    }
}