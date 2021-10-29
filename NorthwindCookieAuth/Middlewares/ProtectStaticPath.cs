using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace NorthwindCookieAuth.Middlewares
{
    // Configuration class
    public class ProtectStaticPathOptions {
        public string Path {get;set;}
        public string PolicyName {get;set;}
    }

    // Middleware class
    public class ProtectStaticPath
    {
        
        private readonly RequestDelegate _next;
        private readonly ProtectStaticPathOptions _options; // This is read from middleware or add service or any configuration added to startup

        public ProtectStaticPath(RequestDelegate next, ProtectStaticPathOptions options)
        {
            _next = next;
            _options = options;
        }

        public async Task InvokeAsync(HttpContext context) {
            // Don't inject the services directly into instance of this class as you will end up with a captive dependency (using a singleton service as scoped and so memory Leak). 
            using (context.RequestServices.CreateScope()) // Creates a new IServiceScope that can be used to resolve scoped services
            {
                // Get Required Service vs Get Service: The first return null in case of non-existant service while the last throw error
                 var _auth=context.RequestServices.GetRequiredService<IAuthorizationService>();
                 if (context.Request.Path.StartsWithSegments(_options.Path)) {
                     var authResult=await _auth.AuthorizeAsync(context.User,context.Request.Path,_options.PolicyName);
                     if (!authResult.Succeeded) {
                         if (context.User==null) {
                             await context.ChallengeAsync();
                         } else {
                             await context.ForbidAsync();
                         }                         
                         return;
                     }
                 }
            }
            await _next(context);
        }
    }

    // Extension class
    public static class Extensions {
        public static IApplicationBuilder UseProtectStaticPath(this IApplicationBuilder application,params ProtectStaticPathOptions[] options) { // params is used when we dont know number of arguments
            foreach (var option in options?? Enumerable.Empty<ProtectStaticPathOptions>())
            {
                application.UseMiddleware<ProtectStaticPath>(option);
            }
            return application;
            
        }
    }
}