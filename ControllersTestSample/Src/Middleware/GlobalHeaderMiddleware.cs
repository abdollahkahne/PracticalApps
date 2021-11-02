using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace ControllersTestSample.Middleware
{
    public class GlobalHeaderMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalHeaderMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // This event happen just right before setting headers to response.
            // There is other event (OnCompleted) which is when the response finished and you want to do for example some logging
            context.Response.OnStarting(() =>
            {
                if (context.Request.Query["and"] == "query")
                {
                    context.Response.Headers.Add("global-header", "This header added by middleware");
                }
                else
                {
                    context.Response.Headers.Add("local-header", "This header added by middleware");
                }

                return Task.CompletedTask;
            });

            await _next(context);

        }
    }
    public static class Extensions
    {
        public static IApplicationBuilder UseGlobalHeader(this IApplicationBuilder app)
        {
            app.UseMiddleware<GlobalHeaderMiddleware>();
            return app;
        }
    }
}