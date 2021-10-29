using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using NorthwindIdentity.Data;
using NorthwindIdentity.Models;

namespace NorthwindIdentity.AuthorizationHandler
{
    public class ContactIsAdminsitrator : AuthorizationHandler<OperationAuthorizationRequirement, Contact>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, Contact resource)
        {
            if (context.User==null) {
                return Task.CompletedTask;
            }
            // Manager can do all Operations
            // if (requirement.Name!=Constants.Approve  && requirement.Name!=Constants.Reject) {
            //     return Task.CompletedTask;
            // }
            if (context.User.IsInRole(Roles.Admin.ToString())) {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}