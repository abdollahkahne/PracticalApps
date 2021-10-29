using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;


namespace NorthwindIntl.ExceptionFilter
{
    public class CustomAuthorizationFilter : Attribute,IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var host=Dns.GetHostEntryAsync(context.HttpContext.Connection.RemoteIpAddress)
            .GetAwaiter().GetResult(); // an alternative for async-await used here!
            Console.WriteLine(host.HostName);
            if (host.HostName.EndsWith("localhost",StringComparison.OrdinalIgnoreCase)) {
                context.Result=new UnauthorizedResult();
            }
        }
    }
}