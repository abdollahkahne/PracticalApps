using System;
using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace NorthwindIntl.ActionConstraints
{
    public class CustomAuthorizationAttribute : Attribute, IActionConstraint
    {
        public int Order => int.MaxValue;

        public bool Accept(ActionConstraintContext context)
        {
            return context.CurrentCandidate.Action.DisplayName.Contains("Authorized");
        }
    }
}