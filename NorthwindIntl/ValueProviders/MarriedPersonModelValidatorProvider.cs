using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace NorthwindIntl.ValueProviders
{
    public partial class Person {
        public string FullName {get;set;}
        public bool Married {get;set;}
        public string PartnerName {get;set;}
    }
    public partial class Person : IValidatableObject
    {
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return Enumerable.Empty<ValidationResult>();
        }
    }
    public class MarriedPersonModelValidatorProvider : IModelValidatorProvider
    {
        public void CreateValidators(ModelValidatorProviderContext context)
        {
            context.Results.Add(new ValidatorItem {Validator=new MarriedPersonModelValidator()});
        }
    }
    public class MarriedPersonModelValidator : IModelValidator
    {
        public IEnumerable<ModelValidationResult> Validate(ModelValidationContext context)
        {
            if (context.Model is Person) {
                var person= context.Model as Person;
                if (person.Married && string.IsNullOrEmpty(person.PartnerName)) {
                    var returnValue=Enumerable.Empty<ModelValidationResult>();
                    var validationResult=new  ModelValidationResult("Married","The Married Person should have Partner name filled!");
                    System.Console.WriteLine(validationResult.Message);
                    return returnValue.Append(validationResult);

                }
            } 
            return Enumerable.Empty<ModelValidationResult>();
        }
    }
}