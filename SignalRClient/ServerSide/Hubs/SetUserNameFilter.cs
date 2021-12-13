using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

#nullable disable
namespace ServerSide.Hubs
{
    public class SetUserNameFilter : IHubFilter // This not do any thing special for now
    {
        private readonly HttpContextAccessor _httpContextAccessor;

        public SetUserNameFilter(HttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Object> InvokeMethodAsync(HubInvocationContext invocationContext, Func<HubInvocationContext, ValueTask<object>> next)
        {
            return await next(invocationContext);
        }
        public async Task OnConnectedAsync(HubLifetimeContext context, Func<HubLifetimeContext, Task> next)
        {
            var claims = new Claim[] { new Claim(ClaimTypes.Name, "John Snow"), new Claim(ClaimTypes.NameIdentifier, "1") };
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);
            var options = new HubConnectionContextOptions();
            var hubContext = context.ServiceProvider.GetRequiredService<HubConnectionContext>();
            // var ctx=new HubConnectionContext(hubContext.ConnectionId,);
            // var ticket = new AuthenticationTicket(user, "SignalR");
            // await _httpContextAccessor.HttpContext.AuthenticateAsync("SignalR");
            // Console.WriteLine(context.Context.User.Identity.Name);
            // await context.Context.GetHttpContext().AuthenticateAsync("SignalR");

            await next(context);
            // Console.WriteLine(context.Context.GetHttpContext().User.Identity.Name);
        }
    }

}