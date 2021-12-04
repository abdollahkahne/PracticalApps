using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using Xunit;

namespace Tests.Helpers
{
    public static class HttpClientExtensions
    {
        // since a form can have more than one submit button we get it from user as method argument
        public static Task<HttpResponseMessage> SubmitFormAsync(this HttpClient httpClient, IHtmlFormElement form, IHtmlElement submitBtn, IEnumerable<KeyValuePair<string, string>> formValues)
        {
            // Use Assert to get data and convert its type to simulatenously check them too
            foreach (var input in formValues)
            {
                var name = input.Key;
                var value = input.Value;
                var inputElement = Assert.IsAssignableFrom<IHtmlInputElement>(form[name]);
                inputElement.Value = value;// do this since page handler gets its data from binding and not from handler input arguments
            }

            var submission = form.GetSubmission(submitBtn);

            var target = (Uri)submission.Target;
            // check if submitBtn is source of sumition
            if (submitBtn.HasAttribute("formaction"))
            {
                var submitAction = submitBtn.GetAttribute("formaction");
                if (!string.IsNullOrEmpty(submitAction))
                {
                    target = new Uri(submitAction, UriKind.Relative);
                }
            }
            var request = new HttpRequestMessage(new HttpMethod(submission.Method.ToString()), target);
            request.Content = new StreamContent(submission.Body);

            // Add all form header to both request and request content header. One of them is enctype
            foreach (var item in submission.Headers)
            {
                request.Headers.TryAddWithoutValidation(item.Key, item.Value);
                request.Content.Headers.TryAddWithoutValidation(item.Key, item.Value);
            }

            return httpClient.SendAsync(request);

        }
    }
}