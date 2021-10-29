using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NorthwindCookieAuth.Filters
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class LoggerAttribute : Attribute, IAsyncPageFilter
    {
        public LoggerAttribute(string logMessage)
        {
            LogMessage = logMessage;

        }
        private EventId _eventId;
        public string LogMessage { get; }
        public LogLevel LogLevel {get;set;}=LogLevel.Information;
        private string GetLogMessage(ModelStateDictionary modelState) {
            var msg=LogMessage;
            foreach (var key in modelState.Keys)
            {
                msg=msg.Replace($"{{key}}",modelState[key].RawValue?.ToString());
            }
            return msg;
        }

        private ILogger GetLogger(PageHandlerExecutingContext context) {
            // For Registering Generic Services in Startup we should register Open Generic Types
            // Like ILogger<> and Logger<> as implementation. But in Constructor use generic type for injection
            // In case of using GetService Or GetRequiredService do following:
            var logger=context.HttpContext.RequestServices
            .GetRequiredService(typeof(ILogger<>).MakeGenericType(context.ActionDescriptor.HandlerTypeInfo.UnderlyingSystemType)) as ILogger;
            return logger;
        }
        public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            
            var page=context.ActionDescriptor;
            _eventId=new EventId(Environment.TickCount,page.DisplayName);
            var loggMessage=GetLogMessage(context.ModelState);
            var logger=GetLogger(context);
            Console.WriteLine(logger.ToString());
            var ctx = await next();
            // var duration=TimeSpan.FromMilliseconds(Environment.TickCount-_eventId.Id);
            logger.Log(LogLevel,_eventId,loggMessage);
        }

        public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
        {
            return Task.CompletedTask;
        }
    }
}