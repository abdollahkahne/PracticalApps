using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NorthwindIntl.ValueProviders
{
    public class LogActionFilterAttribute:ActionFilterAttribute
    {
        
        public override void OnActionExecuting(ActionExecutingContext context) {
            var loggerFactory=context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
            var logger=loggerFactory.CreateLogger(context.Controller.GetType());
            logger.LogDebug($"Before {context.ActionDescriptor.DisplayName} ");
            Console.WriteLine($"Before {context.ActionDescriptor.DisplayName} ");
        }
        
    }
}