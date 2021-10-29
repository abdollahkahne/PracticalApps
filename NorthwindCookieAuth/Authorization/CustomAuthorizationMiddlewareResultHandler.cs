using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;

namespace NorthwindCookieAuth.Authorization
{
    public class CustomAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
    {
        public Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
        {
            Console.WriteLine("AuthorizationMiddlewareResultHandler is handling");
            if (authorizeResult.Succeeded) {
                Console.WriteLine($"Authorization Succeeded For {context.User.Identity.Name} !");
            }
            if (authorizeResult.Forbidden) {
                foreach (var item in policy.Requirements)
                {
                    Console.WriteLine(item.GetType());
                }
            }
            return next(context);
        }
    }
}