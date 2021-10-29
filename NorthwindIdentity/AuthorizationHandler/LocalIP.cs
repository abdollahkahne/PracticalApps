using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace NorthwindIdentity.AuthorizationHandler
{
    public sealed class LocalIPAuthorizationRequirement:IAuthorizationRequirement
    {
        public const string Name="LocalIP";
    }

    public sealed class LocalIPAuthorizationHandler : AuthorizationHandler<LocalIPAuthorizationRequirement>
    {
        private readonly HttpContext _httpContext;

        public LocalIPAuthorizationHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContext = httpContextAccessor.HttpContext;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, LocalIPAuthorizationRequirement requirement)
        {
            var success=IPAddress.IsLoopback(_httpContext.Connection.RemoteIpAddress);
            if (success) {
                context.Succeed(requirement);
            } else {
                context.Fail();
            }
            return Task.CompletedTask;
        }
    }


}