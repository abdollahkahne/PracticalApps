using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace SignalRSample.SignalR.Filters
{
    public class LanguageHubFilterAttribute : Attribute
    {

        public int ArgumentOrder { get; set; }
    }
    public class LanguageHubFilter : IHubFilter
    {
        private readonly ILogger<LanguageHubFilter> _logger;
        private List<string> _bannedWords = new List<string> { "Politics", "Murder" };


        public LanguageHubFilter(ILogger<LanguageHubFilter> logger)
        {
            _logger = logger;
        }


        // This is value Task so result be availabe when it really run and we can not use its result as promise altough it work async like a task
        public async ValueTask<object> InvokeMethodAsync(HubInvocationContext invocationContext, Func<HubInvocationContext, ValueTask<object>> next)
        {
            var context = invocationContext;
            var languageFilter = (LanguageHubFilterAttribute)Attribute.GetCustomAttribute(invocationContext.HubMethod, typeof(LanguageHubFilterAttribute));
            if (languageFilter != null
            && invocationContext.HubMethodArguments.Count > languageFilter.ArgumentOrder
            && invocationContext.HubMethodArguments[languageFilter.ArgumentOrder] is string str)
            {
                foreach (var item in _bannedWords)
                {
                    str = str.Replace(item, "***");
                }
                var newArguments = invocationContext.HubMethodArguments.ToArray();
                newArguments[languageFilter.ArgumentOrder] = str;
                context = new HubInvocationContext(invocationContext.Context, invocationContext.ServiceProvider, invocationContext.Hub, invocationContext.HubMethod, newArguments);
            }
            return await next(context);
        }
        // //Optional Method
        // public async Task OnConnectedAsync(HubLifetimeContext context, Func<HubLifetimeContext, Task> next)
        // {
        //     await next(context);
        //     _logger.LogInformation("Hub Starting a connection with Connection Id:{connectionId}", context.Context.ConnectionId);

        // }
        // //Optional Method
        // Task OnDisconnectedAsync(HubLifetimeContext context, Exception exception, Func<HubLifetimeContext, Exception, Task> next)
        // {
        //     _logger.LogInformation("A Connection to Hub is closing. Connection Id:{connectionId}", context.Context.ConnectionId);
        //     return next(context, exception);
        // }
    }
}