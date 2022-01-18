using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace BlazorWebAssemblySignalRApp.Client.Utilities
{
    // Delegating Handler is a type of message handler which change request or response in some way
    // Apply it to all Request by adding it in configure service using AddHttpMessageHandler
    // In case of SignalR in Blazor Web Assembly we can add it to HubConnection Builder
    public class IncludeRequestCredentialsMessageHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Console.WriteLine("I runnded for this request!");
            // The following setting in case of SignalR are set in Sec-fetch-* (for example Sec-Fetch-Mode=NoCors)
            // request.SetBrowserRequestMode(BrowserRequestMode.NoCors);
            //  set Include on cross-origin fetch requests
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);// To send cookies with CORS request we should enable useCredentials in both end (in front end we should include credentials)
            // Console.WriteLine(request.Headers.Where(h => h.Key.ToLower() == "credentials").FirstOrDefault().Value);
            return base.SendAsync(request, cancellationToken);
        }
    }
}