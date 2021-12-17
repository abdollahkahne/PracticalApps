using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
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
            .WithUrl("http://localhost:5163/chat")
            // .WithUrl("http://localhost:5163/chat", option =>
            // {
            //     // option.UseDefaultCredentials = true; // this make system to use the windows authentication
            //     // Windows authentication is supported in Internet Explorer and Microsoft Edge, but not in all browsers.
            //     // For example, in Chrome and Safari, attempting to use Windows authentication and WebSockets fails.
            //     // When Windows authentication fails, the client attempts to fall back to other transports which might work.
            //     // option.Headers[Microsoft.Net.Http.Headers.HeaderNames.Authorization] = "SignalR";
            // })
            .WithAutomaticReconnect().Build();
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
            if (_hubConnection.State == HubConnectionState.Disconnected)
            {
                await _hubConnection.StartAsync();
            }
            await _hubConnection.InvokeAsync(ServerChatEvents.SendMessage, input);

        }

        // StreamAsync and StreamAsChannelAsync works on methods that return ChannelReader or IAsyncEnumerable and the name is not apply any constraint to the server method return type
        public async Task GetCounterStream(CounterInput counter)
        {
            if (_hubConnection.State == HubConnectionState.Disconnected)
            {
                await _hubConnection.StartAsync();
            }
            Console.WriteLine("Counter Stream Started....");
            // The StreamAsync and StreamAsChannelAsync methods on HubConnection are used to invoke server-to-client streaming methods
            var stream = _hubConnection.StreamAsync<int>(ServerChatEvents.CounterStream, counter); // This returns a IAsyncEnumerable which can be used in client for example using await for
            await foreach (var item in stream)
            {
                Console.Write($"{item}-");
            }
            Console.WriteLine();
            Console.WriteLine("Stream Ended");
        }

        public async Task GetCounterChannelStream(CounterInput counter, CancellationToken cancellationToken)
        {
            if (_hubConnection.State == HubConnectionState.Disconnected)
            {
                await _hubConnection.StartAsync();
            }
            Console.WriteLine($"{DateTime.Now}: Counter Stream Started....");
            // var stream = await _hubConnection.StreamAsChannelAsync<int>(ServerChatEvents.CounterStream, counter); // IAsyncEnumerable and ChannelReader/Writer can convert to each other it seems
            var stream = await _hubConnection.StreamAsChannelAsync<int>("CounterChannel", counter, cancellationToken);
            while (await stream.WaitToReadAsync(cancellationToken))
            {
                var item = await stream.ReadAsync(cancellationToken);
                Console.Write(@"{0}-", item.ToString());
            }
            Console.WriteLine();
            Console.WriteLine($"{DateTime.Now}:Stream Ended");
        }
        public async Task SendChannelStream(CounterInput counter, CancellationToken cancellationToken)
        {
            if (_hubConnection.State == HubConnectionState.Disconnected)
            {
                Console.WriteLine("I am trying to start again...");
                await _hubConnection.StartAsync();
            }

            var channel = Channel.CreateBounded<string>(10);
            await _hubConnection.SendAsync(ServerChatEvents.UploadChannelReader, channel.Reader);
            await writeToChannel(channel.Writer, counter, cancellationToken);
        }
        private async Task writeToChannel(ChannelWriter<string> writer, CounterInput counter, CancellationToken cancellationToken)
        {
            Exception writeException = null;
            try
            {
                await writer.WriteAsync("This is the first item sent from client to server", cancellationToken);
                await Task.Delay(counter.Delay, cancellationToken);
                await writer.WriteAsync("This is the second item sent from client to server which can be received immidiately", cancellationToken);
                await Task.Delay(counter.Delay, cancellationToken);
                await writer.WriteAsync("This is the third item sent from client to server after a delay", cancellationToken);
                await Task.Delay(counter.Delay, cancellationToken);
                await writer.WriteAsync("After this item we complete the writer and so the reader should mark completed!", cancellationToken);
            }
            catch (System.Exception ex)
            {
                writeException = ex;
                Console.WriteLine(ex.Message);
            }
            finally
            {
                writer.Complete(writeException);
            }
        }

        public async Task SendAsyncStream(CounterInput counter, CancellationToken cancellationToken)
        {
            if (_hubConnection.State == HubConnectionState.Disconnected)
            {
                await _hubConnection.StartAsync();
            }

            await _hubConnection.SendAsync(ServerChatEvents.UploadStream, ReturnIAsyncStream(counter, cancellationToken));
        }

        private async IAsyncEnumerable<string> ReturnIAsyncStream(CounterInput counter, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            for (int i = 0; i < counter.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return i.ToString();
                await Task.Delay(counter.Delay, cancellationToken);
            }
            // After the for loop has completed and the local function exits
            // the stream completion will be sent to the server. but in case of 
            // using Chanel reader, use writer.Complite
        }


    }
    public class ServerChatEvents
    {
        public const string SendMessage = "SendMessage";
        public const string CounterStream = "CounterStream";
        public const string UploadChannelReader = "UploadChannelReader";
        public const string UploadStream = "UploadStream";
    }
    public class MessageData
    {
        public string Message { get; set; }
        public string Username { get; set; }
    }
}


// Scale out SignalR App: Why?
// 1- Since the number of TCP Connections are limited (about 65000) we may need more servers.
// 2- In case we have more than on server, the connections in case of not be web socket transportation, forget who were the server which was responsible for connection (none stcky sessions)
// 3- Even if we know which server is responsible for a connection, in case of sending a message to all other clients they do not count other servers open connections!

// So this topic is matter if we are not resterictly using web-socket in client and server side and we have more than one web server
// Also in case of web-scoket we should disable Negotiation to prevent it to do two request


// Sticky Session:
// We can do some practice in case of multiple server (farm) to the every IP/Client get connected to exactly one specified server all the time and requests get handled by the specified server and not the other. 
// This named sticky session and are matter here in case of signalR and also in case of Caching.
// This can do for example by setting a cookie which determine the serever which is responsible for client or adding a Token similarly.

// Limited TCP Connections
// As it saied the simulatenous number of TCP connections can not be more than 65535. consider that this is for all the web application on server and not just our app!
// The standard Http Request are ephemeral connections which means the can be closed when the client get idle and can reopen later
// SignalR connections need to be alive even in case of idle clients (it is the nature of web socket and requirement for Long Polling and SSE) (Persistent connections)
// Persistent connections need some memory to track each connection
// The heavy use of connection-related resources by SignalR can affect other web apps that are hosted on the same server. (So do not share them with other app and host other web app on the other servers!)

// We here reperesent two options for this situation:
// 1- Azure SignalR Service
// 2- Redis backplane

// Azure SignalR Service:
// This is the preferred on in case of hosting the app on Azure 
// The workflow is: 
// 1- client request (authenticate and connect) to the app
// 2- App Server who received the request send a redirect response which redirects to Azure SignalR Service (in this step app server request an auth token from Azure and create redirect url using that token )
// 3- Client Connect to the service to use signalR. The result is that the service manages all of the client connections and the service have a small number of connections to the App Servers

// Consider that here we do not use the sticky session (Client Affinity) and the service is who process requests here (of course have some connection to App Servers but this is not proxy in the middle!)
// It can handle any number of connection without needing to scale out the signalR App Service and in case of being multiple SignalR Server is not matter here in the work
// So A SignalR app won't use significantly more connection resources than a web app without SignalR.

// Redis backplane
// Here we have do the work through the App servers itself
// The Redis backplane is the recommended scale-out approach for apps hosted on your own infrastructure speficially when low latency required.
// Redis is an in-memory key-value store that supports a messaging system with a publish/subscribe model. 
// It has the following features:
// * In case of comming a message from client, it uses the pub/sub feature to forward messages to App servers and after creating connection between clien and one of the app server, the connection information is passed to the backplane.
// * When a server wants to send a message to all/Other/One clients, it sends to the backplane. The backplane knows all connected clients and which servers they're on. It sends the message to all/other/one clients via their respective servers
// It has the following disadvantage:
// 1- It requires sticky session (This is done by load balancer which is a web server for example iis, apache, nginx). 
// 2- The signalR App must scale out depending on the clients number.

// Another problem with deploying SignalR is setting up the reverse proxy web server (other than Kestrel and IIS) to handle the hub address and switching the protocol which can be seen in Microsoft Doc Here:
// https://docs.microsoft.com/en-us/aspnet/core/signalr/scale?view=aspnetcore-6.0


