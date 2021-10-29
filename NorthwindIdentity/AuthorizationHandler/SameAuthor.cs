using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using NorthwindIdentity.Data;

namespace NorthwindIdentity.AuthorizationHandler
{
    // We can define Resource Type as second Generic Type Here (context.resource)
    public sealed class SameAuthorAuthorizationHandler : AuthorizationHandler<SameAuthorAuthorizationRequirement, Book>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SameAuthorAuthorizationRequirement requirement, Book resource)
        {
            // Identity may be null in case of Anynomous Users
            if (resource.Author==context.User.Identity?.Name) {
                context.Succeed(requirement);
            } else {
                context.Fail(); // This is optional
            }
            return Task.CompletedTask;
        }
    }

    public sealed class SameAuthorAuthorizationRequirement:IAuthorizationRequirement {}
}