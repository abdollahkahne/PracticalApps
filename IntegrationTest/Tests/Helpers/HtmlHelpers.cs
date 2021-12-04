using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Html.Dom;
using AngleSharp.Io;

namespace Tests.Helpers
{
    public class HtmlHelpers
    {
        // This function get response and work like a browser to show the html document using following steps:
        // 1- Read content of response as string
        // 2- set document url and status code equal to response
        // 3- set html document headers equal to response and content headers
        // 4- set html document content equal to content read from response.content

        public static async Task<IHtmlDocument> GetDocumentAsync(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            var document = await BrowsingContext.New().OpenAsync(createDocument, CancellationToken.None);
            return document as IHtmlDocument;

            void createDocument(VirtualResponse htmlResponse)
            {
                htmlResponse.Address(response.RequestMessage.RequestUri).Status(response.StatusCode);
                MapHeaders(response.Headers, htmlResponse);
                MapHeaders(response.Content.Headers, htmlResponse);
                htmlResponse.Content(content);

            }
            void MapHeaders(HttpHeaders headers, VirtualResponse htmlResponse)
            {
                foreach (var header in headers)
                {
                    foreach (var value in header.Value)
                    {
                        htmlResponse.Header(header.Key, value);
                    }
                }
            }
        }

    }
}