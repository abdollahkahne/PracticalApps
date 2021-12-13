using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SignalRSample.SignalR.Filters;

namespace SignalRSample.SignalR
{
    // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    // [Authorize]
    public class ChatHub : Hub
    {
        public async Task Send(ChatHubData input)// Methods on server can be called directly from client using connection.invoke("send"); if we want a simpler name we can use [HubMethodName] attribute to use your choice.
        {
            Console.WriteLine("In a Hub method:");
            Console.WriteLine(Context.GetHttpContext().Request.Headers[Microsoft.Net.Http.Headers.HeaderNames.Cookie].FirstOrDefault());




            await Clients.Others.SendAsync("message", new { Username = Context.User.Identity.Name, Message = input.Message });// raise an event name "message" on other clients. To handle this we should add a callback to this custom event
            // . Objects sent as method parameters are deserialized using the configured protocol.
            await Clients.Caller.SendAsync("sent"); // send a notification to caller that his/her msg sent to all other clients
            await Clients.All.SendAsync("notification", new { UserId = Context.UserIdentifier });// Make a notification to all clients in hub that some one had an action
            // By default, SignalR uses the ClaimTypes.NameIdentifier from the ClaimsPrincipal associated with the connection as the user identifier.
            // Caller is a user or connection? connection. but we can send to special user even he/she has multiple connection on Mobile/PC/Tablet simulatenousely.
            await Clients.User(Context.UserIdentifier).SendAsync("sent");// The user identifier is case-sensitive.
            // Altough user id is the Name Identifier Claim, it can change to something else. we introduce a method for this later

        }

        [Authorize(Policy = "DevelopersAdmin")]
        public async Task SendToDevelopers(ChatHubData input)
        {
            await Clients.Group("Developers").SendAsync("message", new { Title = "AnynomousTest", Message = input.Message });
        }

        public Task<ChatHubData> GetUserEmail()
        {
            // In case of throwing an error by method, a default error sent to clients. If you want to sent more special error sent a HubException error to sent it altough it is recommended to sent details from server to client carefully.
            // throw new HubException("John snow has no email as exception!");
            return Task.FromResult(new ChatHubData { Message = "john snow has no email!" });
        }

        [LanguageHubFilterAttribute(ArgumentOrder = 0)]
        public async Task SendSensored(string message)
        {
            await Clients.All.SendAsync("message", new { Message = message });
        }

        public async override Task OnConnectedAsync()
        {
            Console.WriteLine("After Connected To Hub:");
            Console.WriteLine(Context.GetHttpContext().Request.Headers[Microsoft.Net.Http.Headers.HeaderNames.Cookie].FirstOrDefault());
            // Groups is a Group Manager and not the client.Groups
            await Groups.AddToGroupAsync(connectionId: Context.ConnectionId, groupName: "Developers");
            await Clients.Group("Developers").SendAsync("AddToDevelopers", new { Username = Context.User.Identity.Name, ConnectionId = Context.ConnectionId });
            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            if (exception != null)
            {
                Console.WriteLine(exception.Message);
            }
            return base.OnDisconnectedAsync(exception);
        }

    }

    public class ChatHubData
    {
        public string Message { get; set; }
        public string Username { get; set; }
    }

    //  We should register this in service container

    public class NameUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            // Change to use Name as the user identifier for SignalR
            // WARNING: This requires that the source of your JWT token 
            // ensures that the Name claim is unique!
            // If the Name claim isn't unique, users could receive messages 
            // intended for a different user!
            return connection.User.Claims.Where(c => c.Type == ClaimTypes.Name).FirstOrDefault()?.Value;
        }
    }


    // Authorize using Authorization handler is a way to secure the method or Hub altough it does not set User value of Hub here
    // Usually make a requirement separate and then make the handler using interface
    public class RestrictedRequirement : AuthorizationHandler<RestrictedRequirement, HubInvocationContext>, IAuthorizationRequirement
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RestrictedRequirement requirement, HubInvocationContext resource)
        {
            if (isUserAllowed(resource.HubMethodName, context.User.Identity.Name))
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }

        private bool isUserAllowed(string hubMethodName, string username)
        {
            return (hubMethodName.Equals("SendToDevelopers", StringComparison.OrdinalIgnoreCase) && username.Equals("john@gmail.com"));
        }
    }

}

// A hub is a high-level pipeline that allows a client and server to call methods on each other. 
// SignalR handles the dispatching across machine boundaries automatically, allowing clients to call methods on the server and vice versa.
// You can pass strongly-typed parameters to methods, which enables model binding.
// SignalR provides two built-in hub protocols: a text protocol based on JSON and a binary protocol based on MessagePack (Something similar with json but more compact/fast and for small data)
// Consider that like a thing that client can gather around it.
// On the client side we have the concept of HubConnection which create a connection between client and Hub

// Hubs are following Properties:
// Clients: which stand for Clients who connected to this hub in Terms of User,Client,Group and also we have All,Caller,Others clients,
// All of this batch of clients have an SendAsync method which can be used to Get a method on client side using callback.
// Context: which stands for context of clients which include connection ID and user info and Items for sharing state between multiple method
// Groups: which is Group Manager for working with groups
// OnConnectedAsync: which we can set using it a callback on server to do when connection started. For example add the connected user to some groups.
// OnDisconnectedAsync: similar to above case

// Exposing ConnectionId can lead to malicious impersonation if the SignalR server or client version is ASP.NET Core 2.2 or earlier: Try to not send it to client side since it have itself Id on client

// Question: What if you want to rais an event in client from controller or middlewares or other components?
// Answer: There is a service related to Hub which make it availabe to invoke an event in client side. To do this
// you should inject an instance of IHubContext<T> (T is a Hub itself here) which you can use it like the Clients property of Hub to raise event in different sets of client (Groups or All, we have not Caller concept here! we should use Context of HttpContext here instead of context) (It has the Group Manager property too add someone to group or remove from group too but i tcan only be called in server side). 
// Using it in javascript is similar to Hub (using inject method). But since this service is only Client part of Hub it has not the option to call a method in controller/middleware from client side. 
// There is an interface IHubContext without generic hub. This can not be injected but can be used in Libraries and method and inputs from IHubContext<T> can cast to them for more usage (Used in Extensions and Libraries)

// some information about group:
// Group membership isn't preserved when a connection reconnects. The connection needs to rejoin the group when it's re-established.
// It's not possible to count the members of a group, since this information is not available if the application is scaled to multiple servers. So how telegram show group member?
// I think they have another entity which after connection and disconnection user added to that entity and for counting the user that entity counted!

// What about authorization and authentication:
// It is available in group. If a user is added to a group only when the credentials are valid for that group, messages sent to that group will only go to authorized users. // This is a concept which we study later. for now there is no authentication 
// However, groups are not a security feature. Authentication claims have features that groups do not, such as expiry and revocation.
// For example we have a controller that is authenticated and it called a IHubContext which send some data from that controller. What happened?
// Answer: The data sent to all user including authenticated and anynomous! which should be considered when using that.


// Recommended Pattern for Designing Server/Client side:
// In server write the methods with only one object (class) and when client invoke it, it should send an object which automaticly bind to the object properties in server. This way the order or properties doesnt matter and we can have more or less properties
// In writing client send event handler too do the similar. for example we should have connection.on("eventname",function(data) {}). And we have the same in server side as ...SendAsync("eventname",data).
// If our methods in Hub return value, wrap them in an object similary. Of course we better to use in javascript functions deconstruction and object building

// Hub filters:
// Are available in ASP.NET Core 5.0 or later.
// Allow logic to run before and after hub methods are invoked by clients.
// Hub filters can be applied globally or per hub type.
// The order in which filters are added is the order in which the filters run. Global hub filters run before local hub filters.
// They can run when service registeration (both globals and locals). Global defined inside addSignalR and the locals chained with it using AddHubOption<Thub>()
// The filters can be registered using AddFilter<TFilter>() [Concrete Type], AddFilter(typeof(TFilter)) [Runtime type] or AddFilter(new TFilter()); // The third one is singleton and can save the state
// in case 1,2 (using Types) we should register filers in DI separately too. which can be singleton/Scoped/Transient.
// Hub filters are created and disposed per hub invocation. If you want to store global state in the filter, or even no state, add the hub filter type to DI as a singleton for better performance.
// Alternatively, add the filter as an instance if you can.
// Create a filter by declaring a class that inherits from IHubFilter, and add the InvokeMethodAsync method (this can run before and after method invocation).
// There is also OnConnectedAsync and OnDisconnectedAsync that can optionally be implemented to wrap the OnConnectedAsync and OnDisconnectedAsync hub methods respectively.


//  The typical lifecycle of a WebSocket interaction between client & server goes as follows:
// Client initiates a connection by sending an HTTP(S) WebSocket handshake request.
// Server replies with a handshake response (all handled by the web application server transparently to the application) and sends a response status code of 101 Switching Protocols.

// From that point on both browser and server communicate using WebSocket API with a completely symmetrical connection (each party can send and retrieve text and binary messages)
// WebSockets are not restrained by the same-origin policy (In Browser it means. In server altough we add UseCors but with Postman we can see it since browser control it to prevent unwanted Cross Site requests )
//
// Negotiation Step Request and Response:
// Post:https://localhost:5001/chathub/negotiate?negotiateVersion=1
// Headers are similar to every web request including cookies and authorization if required and settings done in Javascript to create the hub connection
// Response include connection Id, connection Token and Available Transports in server side and its transfer formats related to each of them:
// { "negotiateVersion":1,"connectionId":"a3crMDJ-RgSFcGYBIhrOcA","connectionToken":"HIqboTOp2F1sP0JI8AJoZw","availableTransports":[{ "transport":"WebSockets","transferFormats":["Text","Binary"]},{ "transport":"ServerSentEvents","transferFormats":["Text"]},{ "transport":"LongPolling","transferFormats":["Text","Binary"]}]}

// We can remove the negotiation step which result to different handshaking request (using SkipNegotiation=true in javascript). Consider here that it is false (which is by default)
// HandShaking Step and Socket messages
// A Get Request to GET /chathub?id=N0ZL4uSyO-e7V8FcyB9bDw&access_token=123456 HTTP/1.1
// It is a http request which the id is connectionToken returned in previous step and other query string generated by accessTokenFactory which configured in javascript. For more query string you can add them to hub url in MapHub or add them to accessToken itself using & 
// The header specified in javascript header does not included here but cookies are included automaticly. There is following interesting headers:
// Connection:	keep-alive, Upgrade
// Upgrade: websocket
// Sec-WebSocket-Version: 13
// Sec-WebSocket-Extensions: permessage-deflate
// Sec-WebSocket-Key: HlQDt7GPnFP2+OLvCGeyHg==

// And the response is 101 which means switch the protocol (I described what is the websocket key, web-socket accept is in this request/response header)
// HTTP/1.1 101 Switching Protocols
// Connection: Upgrade
// Date: Fri, 10 Dec 2021 22:57:02 GMT
// Server: Kestrel
// Upgrade: websocket
// Sec-WebSocket-Accept: ZAoj+qGBntY8a/Id2IW7kpYWy7s=

// The Sec-WebSocket-Accept header is important in that the server must derive it from the Sec-WebSocket-Key that the client sent to it.
// To get it, concatenate the client's Sec-WebSocket-Key and the string "258EAFA5-E914-47DA-95CA-C5AB0DC85B11" together (it's a "magic string"),
// take the SHA-1 hash of the result, and return the base64 encoding of that hash.

// After this successful return browser sends ws or wss requests to the related hub. Web socket does not support anything else than the url so the header and the cookie are not available in hubs by default. and the response showed in browser as follows:
// {"protocol":"json","version":1} // as it seems it determined the web socket serialization protocol
// {"type":1,"target":"AddToDevelopers","arguments":[{"username":"jon@example.com","connectionId":"OrJt_bkKT_tzRRiFU2AZdw"}]} // the type of event and its argument in server to clients
// {"arguments":[],"invocationId":"0","target":"GetUserEmail","type":1}{"arguments":[{"message":"sample input"}],"invocationId":"1","target":"Send","type":1}{"arguments":["Internal Politics Has increase Murders"],"invocationId":"2","target":"SendSensored","type":1}{"arguments":[{"message":"This is a test for authorization in groups"}],"invocationId":"3","target":"SendToDevelopers","type":1}{"arguments":[{"message":"Using send method to trigger a method on server. consider that this only raise the event and do not wait for event handler result"}],"target":"Send","type":1} // This is all the invoke from client side to the server side
// {"type":3,"invocationId":"0","result":{"message":"john snow has no email!","username":null}}{"type":1,"target":"sent","arguments":[]}{"type":1,"target":"notification","arguments":[{"userId":"jon@example.com"}]}{"type":1,"target":"sent","arguments":[]}{"type":3,"invocationId":"1","result":null}{"type":1,"target":"message","arguments":[{"message":"Internal *** Has increase ***s"}]}{"type":3,"invocationId":"2","result":null}{"type":3,"invocationId":"3","error":"Failed to invoke 'SendToDevelopers' because user is unauthorized"}{"type":1,"target":"sent","arguments":[]}{"type":1,"target":"notification","arguments":[{"userId":"jon@example.com"}]}{"type":1,"target":"sent","arguments":[]} // response send from server as result of the following requests
// {"type":6} // pings that send by client or server on time interval to announce themselves alive!
// Types has following values (Defined by Microsoft and Not unique in socket!):
// Invocation = 1
// Indicates the message is an Invocation message and implements the InvocationMessage interface.
// StreamItem = 2 
// Indicates the message is a StreamItem message and implements the StreamItemMessage interface.
// Completion = 3 	
// Indicates the message is a Completion message and implements the CompletionMessage interface.
// StreamInvocation = 4 
// Indicates the message is a Stream Invocation message and implements the StreamInvocationMessage interface.
// CancelInvocation = 5 
// Indicates the message is a Cancel Invocation message and implements the CancelInvocationMessage interface.
// Ping = 6 	
// Indicates the message is a Ping message and implements the PingMessage interface.
// Close = 7 
// Indicates the message is a Close message and implements the CloseMessage interface.