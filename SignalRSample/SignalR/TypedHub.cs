using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace SignalRSample.SignalR
{
    public class TypedHub : Hub<IMyTypedHub>
    // when using Hub<T> all the client sets including All, Other, Caller and ... Have type of T so
    // we can use for example Clients.Caller.MessageDelivered which has signature like that defined in interface.
    // and it is equal to using Clients.Caller.SendAsync("MessageDelivered",..) which has a parameter more than signature for event name which raises
    {
        public async Task Send(string message)
        {
            await Clients.Others.Message(Context.User.Identity.Name, message);
            await Clients.Caller.Sent();
            await Clients.All.Notification(Context.UserIdentifier);

        }
    }
    public interface IMyTypedHub
    {
        Task MessageDelivered(string sender, string recipient, string msg);
        Task Notification(string username);
        Task Sent();
        Task Message(string sender, string message);
    }
}