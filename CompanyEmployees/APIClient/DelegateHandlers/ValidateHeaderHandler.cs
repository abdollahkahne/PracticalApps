using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace APIClient.DelegateHandlers
{
    public class ValidateHeaderHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequest, CancellationToken cancellationToken)
        {
            if (httpRequest.Headers.Contains("X-API-KEY"))
            {
                Console.WriteLine("Please Privide API Key"); // Just testing here
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(
                    "The API key header X-API-KEY is required.")
                };
            }
            else
            {
                Console.WriteLine("X-API-KEY Provided");
            }
            return await base.SendAsync(httpRequest, cancellationToken);
        }

    }
}