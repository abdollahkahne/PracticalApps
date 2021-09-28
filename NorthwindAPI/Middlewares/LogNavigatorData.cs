using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Routing;

namespace PracticalApp.NorthwindAPI.Middlewares
{
    public class LogNavigatorData
    {
        private readonly RequestDelegate _next;
        private readonly LinkGenerator _linkGenerator; // this is used for generating links for endpoints

        public LogNavigatorData(RequestDelegate next, LinkGenerator linkGenerator)
        {
            _next = next;
            _linkGenerator = linkGenerator;
        }
        public async Task Invoke(HttpContext context)
        {
            var userAgent = context.Request.Headers["User-Agent"];
            var ip = context.Connection.RemoteIpAddress;
            Console.WriteLine(userAgent.ToString());
            Console.WriteLine(ip.ToString());

            // Unique Identifier for Request
            Console.WriteLine(context.TraceIdentifier);

            // Generate RouteUrl using Link Generator Dependency Injection (Learning)
            var path=_linkGenerator.GetPathByAction(context,action:"getCustomers",controller:"Customers");
            var url=_linkGenerator.GetUriByAction(context,"getCustomers","Customers");
            Console.WriteLine(path);
            Console.WriteLine(url);


            // var payload=context.Request.Form["Payload"].SingleOrDefault();
            // Console.WriteLine(payload);

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