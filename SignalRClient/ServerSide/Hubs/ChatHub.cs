using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;

#nullable disable
namespace ServerSide.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(MessageInput input)
        {
            //  Use await syntax to wait for the server method to complete and try...catch syntax to handle errors.
            await Clients.All.SendAsync(ChatEvents.Message, new { Message = input.Messsage, Username = Context.GetHttpContext().User.Identity.Name });
        }
    }

}