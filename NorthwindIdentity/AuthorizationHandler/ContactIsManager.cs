using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using NorthwindIdentity.Data;
using NorthwindIdentity.Models;

// Why a requirement Like OperationAuthorizationRequirement need to handled by multiple Handlers?
// In cases where you want evaluation to be on an OR basis, implement multiple handlers for a single requirement.
namespace NorthwindIdentity.AuthorizationHandler
{
    public class ContactIsManager : AuthorizationHandler<OperationAuthorizationRequirement, Contact>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, Contact resource)
        {
            if (context.User==null || resource==null) {
                return Task.CompletedTask;
            }
            // Manager can only do Approve Or Reject (He can not do CRUD for other people)
            if (requirement.Name!=Constants.Approve  && requirement.Name!=Constants.Reject) {
                return Task.CompletedTask;
            }
            if (context.User.IsInRole(Roles.Manager.ToString())) {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}