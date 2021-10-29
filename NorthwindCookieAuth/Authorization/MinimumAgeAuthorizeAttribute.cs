using System;
using Microsoft.AspNetCore.Authorization;

namespace NorthwindCookieAuth.Authorization
{
    internal class MinimumAgeAuthorizeAttribute:AuthorizeAttribute
    {
        const string POLICY_PREFIX="MinimumAge";
        // Get or Set Age corresponding to Policy Property of Authorize Attribute
        public int Age {
            get {
                var isInt=int.TryParse(Policy.Substring(POLICY_PREFIX.Length),out int age);
                if (isInt) {
                    return age;
                }
                return default(int);
            }
            set {
                Policy=$"{POLICY_PREFIX}{value.ToString()}";
            }
        }

        public MinimumAgeAuthorizeAttribute(int age)
        {
            Age = age;
        }
    }
}