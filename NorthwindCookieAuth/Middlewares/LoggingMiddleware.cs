using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace NorthwindCookieAuth.Middlewares
{
    public static class MiddlewareExtensions {
        public static IApplicationBuilder UseLoggingMiddleware(this IApplicationBuilder builder) {
            builder.UseMiddleware<LoggingMiddleware>();
            return builder;
        }
    }
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context) {
            // instead of ILogger we can inject LoggerFactory and then use its CreateLogger<T> method

            using (var scope=_logger.BeginScope<LoggingMiddleware>(this))
            {
                 _logger.LogInformation("Logs Before Middleware");
                 await _next(context);
                 _logger.LogInformation("Logs After Middleware");
            }
        }
    }
}