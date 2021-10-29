using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace NorthwindCookieAuth.Middlewares
{
    public class DiagnosticMiddleware
    {
        private readonly RequestDelegate _next;
        //This shows the standard way to log using a DiagnosticSource.
        // You inject the DiagnosticSource into the constructor of the middleware for use when the middleware executes. 
        private readonly DiagnosticSource _diagnostic;

        public DiagnosticMiddleware(RequestDelegate next, DiagnosticSource diagnostic)
        {
            _next = next;
            _diagnostic = diagnostic;
        }

        public async Task InvokeAsync(HttpContext context) {
            await _next(context);

            // When you intend to log an event, you first check that
            // there is a listener for the specific event.
            if (_diagnostic.IsEnabled("DiagnosticMiddleware.MiddlewareStarted")) {
                // In order to create the log, you use the Write method,
                // providing the event name and the data that should be logged
                _diagnostic.Write("DiagnosticMiddleware.MiddlewareStarted",new {
                    httpContext=context,
                });
            }
            
        }
    }
}