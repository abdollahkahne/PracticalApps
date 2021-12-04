using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Src.Data.IdentityModels;

namespace Src.IdentityPolicies
{
    public class CustomUsernameEmailPolicy : IUserValidator<AppUser>
    {
        public async Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user)
        {
            var errors = new List<IdentityError>();
            var alreadyExist = await manager.FindByEmailAsync(user.Email);
            if (alreadyExist != null && alreadyExist.NormalizedUserName != user.NormalizedUserName)
            {
                errors.Add(new IdentityError { Description = "This email already taken" });
            }
            if (user.UserName.ToLower().Contains("admin"))
            {
                errors.Add(new IdentityError { Description = "User name should not contain admin word" });
            }
            if (user.Email.ToLower().EndsWith("yahoo.com"))
            {
                errors.Add(new IdentityError { Description = "Please provide other emails than Yahoo! Provider" });
            }

            if (errors.Count == 0)
            {
                return IdentityResult.Success;
            }
            else
            {
                return IdentityResult.Failed(errors.ToArray());
            }
        }
    }
}