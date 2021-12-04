using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Src.Data.IdentityModels;

namespace Src.IdentityPolicies
{
    public class CustomPasswordPolicy : PasswordValidator<AppUser>
    {
        public override Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user, string password)
        {
            // await base.ValidateAsync(manager, user, password); // This is not needed since framework uses all validator in chain an aggregate the result
            var errors = new List<IdentityError>();
            if (password.ToLower().Contains(user.UserName.ToLower()))
            {
                errors.Add(new IdentityError { Description = "Password should not contain username" });
            }
            if (IsDigit(password.ToCharArray()[0]))
            {
                errors.Add(new IdentityError { Description = "Password should not start with digits" });
            }
            if (errors.Count == 0)
            {
                return Task.FromResult(IdentityResult.Success);
            }
            else
            {
                return Task.FromResult(IdentityResult.Failed(errors.ToArray()));
            }

        }
    }
}