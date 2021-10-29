using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace NorthwindMVC.Middlewares
{
    // (Deprecated) To add custom handlers to HttpClient, use the HttpClientFactory.Create method (This method does not exist in dotnet Core).
    // To Apply it to all Request add it in configure service using AddHttpMessageHandler
    public class TraceIdentifierMessageHandler:DelegatingHandler
    {
        // HttpContext isn't thread-safe. Reading or writing properties of the HttpContext
        // outside of processing a request can result in a NullReferenceException.
        // o avoid unsafe code, never pass the HttpContext into a method that performs background work. Pass the required data instead
        // Here we do async processing and using delegating handler (which finished before starting the next) so it should not be the case
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TraceIdentifierMessageHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellationToken) {
            var context=_httpContextAccessor.HttpContext;
            request.Headers.Add("Request-Id",context.TraceIdentifier);
            // request.Headers.Add("x-Session-Id",context.Session.Id);
            Console.WriteLine("I am running on client side");
            return await base.SendAsync(request,cancellationToken);
        }
        
    }
}