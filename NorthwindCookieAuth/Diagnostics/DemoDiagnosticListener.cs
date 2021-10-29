using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DiagnosticAdapter;

namespace NorthwindCookieAuth.Diagnostics
{
    // To create a listener, you can create a POCO class that contains a method designed to accept parameters of the appropriate type.
    // You then decorate the method with a [DiagnosticName] attribute, providing the event name to listen for: 
    public class DemoDiagnosticListener
    {
        [DiagnosticName("DiagnosticMiddleware.MiddlewareStarted")]
        public virtual void OnMiddlewareStarted(HttpContext httpContext) {
            // the OnMiddlewareStarted() method is configured to handle the "DiagnosticMiddleware.OnMiddlewareStarted" diagnostic event
            // Fired By this name every where (For in this case we fired it in related middleware).
            // The HttpContext, that is provided when the event is logged (fired) is passed to the method
            // as it has the same name, httpContext that was provided when the event was logged (Fired).
            System.Console.WriteLine("Middleware Started");
            System.Console.WriteLine(httpContext.Request.Path.ToString());
            var id=httpContext.TraceIdentifier;
            var id2=httpContext.Features.Get<IHttpRequestIdentifierFeature>().TraceIdentifier;
            Activity.DefaultIdFormat=ActivityIdFormat.W3C;
            Activity.ForceDefaultIdFormat=true;
            var id3=Activity.Current.TraceId;
            var id4=Activity.Current.Id;
            System.Console.WriteLine(@"id is {0} and id2 is {1} and id3 is {2} and id4 is {3}",id,id2,id3,id4);
        
        }
    }
}