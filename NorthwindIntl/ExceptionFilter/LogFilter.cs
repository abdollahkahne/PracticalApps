using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace NorthwindIntl.ExceptionFilter
{
    public class LogFilter : IAsyncActionFilter
    {
        private readonly ILoggerFactory _loggerFactory;

        public LogFilter(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var logger=_loggerFactory.CreateLogger(context.Controller.GetType());
            logger.LogDebug($"{context.ActionDescriptor.DisplayName} action is called!");
            Console.WriteLine($"{context.ActionDescriptor.DisplayName} action is called!");
            await next(); // here next() return action result and if we want to do sth after execution we can add it after next using async await
            logger.LogDebug($"{context.ActionDescriptor.DisplayName} action was called!");
            Console.WriteLine($"{context.ActionDescriptor.DisplayName} action was called!");
        }
    }
}