using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;
using NorthwindIdentity.Models;

namespace NorthwindIdentity.AuthorizationHandler
{
    public sealed class ContactIsResourceOwner : AuthorizationHandler<OperationAuthorizationRequirement, Contact>
    {
        private readonly UserManager<IdentityUser> _userManager;

        public ContactIsResourceOwner(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, Contact resource)
        {
            if (context.User==null || resource==null) 
            {
                // Returning Task.CompletedTask without a prior call to context.
                // Success or context.Fail, is not a success or failure,
                // it allows other authorization handlers to run
                // So only use context.Fail if you want to break handler pipeline (If you need to explicitly fail, call context.Fail.)
                return Task.CompletedTask;
            }
            // Only User can do CRUD operation
            var operation=requirement.Name;
            // Here the better case is to write all the limited case by making extension on allowable operation
            // (operation!=Constants.Read && ...)
            if (operation==Constants.Approve || operation==Constants.Reject) {
                return Task.CompletedTask;
            }
            // Console.WriteLine(@"{0}:{1}",resource.OwnerID,_userManager.GetUserId(context.User));
            if (resource.OwnerID==_userManager.GetUserId(context.User)) {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}