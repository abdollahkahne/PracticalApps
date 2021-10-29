using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace NorthwindCookieAuth.Authorization
{
    public class MultiRequirementAuthorizationhandler : IAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            var pendingRequirements=context.PendingRequirements.ToList();
            foreach (var item in pendingRequirements)
            {
                if (item is MinimumAgeRequirement minimumAge ) {
                    if (context.User.HasClaim(c => c.Type=="Age")) {
                        var age=Convert.ToInt32(context.User.Claims.SingleOrDefault(c =>c.Type=="Age").Value);
                        if (age>=minimumAge.MinimumAge) {
                            context.Succeed(item);
                        } else {
                            context.Fail();
                        }
                    } else {
                        context.Fail(); // Only Use this if you want to UnAuthorize 
                    }
                    
                }
                if (item is ForbidenCountryRequirement forbidenCountry){
                    if (context.User.HasClaim(c => c.Type=="Country")) {
                        var country=context.User.Claims.SingleOrDefault(c => c.Type=="Country").Value;
                        var forbidenCountries=forbidenCountry.ForbidenCountry;
                     
                        if (forbidenCountries.Contains(country)) {
                            context.Fail();
                        } else {
                            context.Succeed(item);
                        }
                    }
                }
            }
            return Task.CompletedTask;
        }
    }

    public class ForbidenCountryRequirement:IAuthorizationRequirement
    {
        public ForbidenCountryRequirement(string[] forbidenCountry)
        {
            ForbidenCountry = forbidenCountry;
        }

        public string[] ForbidenCountry {get;set;}
        
    }

    public class MinimumAgeRequirement:IAuthorizationRequirement {
        public int MinimumAge {get;set;}

        public MinimumAgeRequirement(int minimumAge)
        {
            MinimumAge = minimumAge;
        }
    }
}