using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace NorthwindCookieAuth.Authorization
{
    public class MinimumAgePolicyProvider : IAuthorizationPolicyProvider
    {
        private DefaultAuthorizationPolicyProvider _backupAuthorizationPolicyProvider {get;}
        const string POLICY_PREFIX = "MinimumAge";

        public MinimumAgePolicyProvider(IOptions<AuthorizationOptions> options)
        {
            _backupAuthorizationPolicyProvider=new DefaultAuthorizationPolicyProvider(options);
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        {
            var builder=new AuthorizationPolicyBuilder(CookieAuthenticationDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser();
            return Task.FromResult(builder.Build());
        }

        public Task<AuthorizationPolicy> GetFallbackPolicyAsync()
        {
            return Task.FromResult<AuthorizationPolicy>(null);
        }

        public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            // This Policy should handle in case of start with Policy_Prefix and end with an integer
            if (policyName.StartsWith(POLICY_PREFIX,StringComparison.OrdinalIgnoreCase) && 
            int.TryParse(policyName.Substring(POLICY_PREFIX.Length),out int age)) {
                var policyBuilder=new AuthorizationPolicyBuilder(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddRequirements(new MinimumAgeRequirement(age));
                return Task.FromResult(policyBuilder.Build());
            }
            // use the BackupPolicyProvider instead of returning null:
            // return Task.FromResult<AuthorizationPolicy>(null);
            return _backupAuthorizationPolicyProvider.GetPolicyAsync(policyName);
        }
    }
}