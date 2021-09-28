using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace NorthwindIntl.ValueProviders
{
    public class ValidationMethods
    {
        // We should add this validation method as custom attribute as below:
        // [CustomValidation(typeof(ValidationMethods),"ValidateEmail")]
        public static ValidationResult ValidateEmail(string email,ValidationContext context) {
            if (!string.IsNullOrEmpty(email)) {
                if (!Regex.IsMatch(email,@"^([\w\.\-]+)@([\w\-]+)(\.[\w]{2,3})$")) {
                    return new ValidationResult("Invalid Email",new [] {context.MemberName});
                }
            } 
            return ValidationResult.Success;
        }
    }
}