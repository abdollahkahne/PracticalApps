using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace SignalRSample.SignalR.Filters
{
    public class MyHubFilter : IHubFilter
    {
        private readonly ILogger<MyHubFilter> _logger;

        public MyHubFilter(ILogger<MyHubFilter> logger)
        {
            _logger = logger;
        }

        // This is value Task so result be availabe when it really run and we can not use its result as promise altough it work async like a task
        public async ValueTask<object> InvokeMethodAsync(HubInvocationContext invocationContext, Func<HubInvocationContext, ValueTask<object>> next)
        {

            Console.WriteLine("Before execution of hub method:");
            Console.WriteLine(invocationContext.Context.GetHttpContext().Request.Headers[Microsoft.Net.Http.Headers.HeaderNames.Cookie].FirstOrDefault());


            Console.WriteLine("Invoking Hub Method {0}", invocationContext.HubMethodName);

            try
            {
                return await next(invocationContext);
            }
            catch (System.Exception)
            {
                Console.WriteLine($"Exception in Invoking the method:{invocationContext.HubMethodName}");

                throw;
            }
        }
        //Optional Method
        public async Task OnConnectedAsync(HubLifetimeContext context, Func<HubLifetimeContext, Task> next)
        {
            Console.WriteLine("Before Connecting to Hub:");
            Console.WriteLine(context.Context.GetHttpContext().Request.Headers[Microsoft.Net.Http.Headers.HeaderNames.Cookie].FirstOrDefault());


            await next(context);
            _logger.LogInformation("Hub Starting a connection with Connection Id:{connectionId}", context.Context.ConnectionId);

        }

        Task OnDisconnectedAsync(HubLifetimeContext context, Exception exception, Func<HubLifetimeContext, Exception, Task> next)
        {
            _logger.LogInformation("A Connection to Hub is closing. Connection Id:{connectionId}", context.Context.ConnectionId);
            return next(context, exception);
        }
    }
}