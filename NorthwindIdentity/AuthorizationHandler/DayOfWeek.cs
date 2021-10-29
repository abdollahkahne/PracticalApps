using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace NorthwindIdentity.AuthorizationHandler
{
    // This Authorization Handler should added to startup configureservices as following:
    // It can be added as many times we need
    // services.AddSingleton<IAuthorizationHandler,DayOfWeekAuthorizationHandler>();
    public sealed class DayOfWeekAuthorizationHandler : AuthorizationHandler<DayOfWeekAuthorizationRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, DayOfWeekAuthorizationRequirement requirement)
        {
            // context.Resource Get from Request context and its type should be specified
            // Requirement is what checked by handler and is a defined class by ourselves
            if ((context.Resource is DayOfWeek dayOfWeek) && (dayOfWeek==requirement.DayOfWeek)) {
                context.Succeed(requirement);
            } else {
                context.Fail();
            }
            return Task.CompletedTask;
        }
    }

    public sealed class DayOfWeekAuthorizationRequirement:IAuthorizationRequirement {
        public DayOfWeekAuthorizationRequirement(DayOfWeek dayOfWeek)
        {
            DayOfWeek = dayOfWeek;
        }

        public DayOfWeek DayOfWeek {get;}
        
    }
}