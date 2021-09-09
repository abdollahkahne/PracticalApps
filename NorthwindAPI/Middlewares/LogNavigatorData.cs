using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace PracticalApp.NorthwindAPI.Middlewares
{
    public class LogNavigatorData
    {
        private readonly RequestDelegate _next;

        public LogNavigatorData(RequestDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(HttpContext context)
        {
            var userAgent = context.Request.Headers["User-Agent"];
            var ip = context.Connection.RemoteIpAddress;
            Console.WriteLine(userAgent.ToString());
            Console.WriteLine(ip.ToString());

            // we can do Following Middlewares:
            // 1- Content Generation (Like this)
            // 2- Short Circuiting
            // 3- Request Editing
            // 4- Response Editing

            await _next(context);

            Console.WriteLine("We can do more edit here in backing response to client!");
        }
    }
    public static class UseLogNavigatorDataExtension
    {
        public static IApplicationBuilder UseLogNavigationData(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LogNavigatorData>();
        }
    }

}