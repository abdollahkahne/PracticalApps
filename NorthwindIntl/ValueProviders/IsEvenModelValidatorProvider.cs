using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace NorthwindIntl.ValueProviders
{
    public class IsEvenModelValidatorProvider : IModelValidatorProvider
    {
        public void CreateValidators(ModelValidatorProviderContext context)
        {
            var type=context.ModelMetadata.ModelType;
            if (type==typeof(string) ||
                type==typeof(int) || type==typeof(uint) ||
                type==typeof(short) || type==typeof(ushort) ||
                type==typeof(long) || type==typeof(ulong) ||
                type==typeof(float) || type==typeof(double)
            ) {
                if (!context.Results.Any(v =>v.Validator is IsEvenModelValidator)) {
                    context.Results.Add(new ValidatorItem {
                        Validator=new IsEvenModelValidator(),
                        IsReusable=true,
                    });
                }
            }
        }
    }

    public class IsEvenModelValidator : IModelValidator
    {
        public IEnumerable<ModelValidationResult> Validate(ModelValidationContext context)
        {
            if (context.Model!=null) {
                try
                {
                     var value=Convert.ToDouble(context.Model);
                     if (value%2==0) {
                         yield break;
                     }
                }
                catch (System.Exception)
                {
                    
                }
            }

            yield return new ModelValidationResult(context.ModelMetadata.PropertyName,$"{context.ModelMetadata.PropertyName} is not even!");
        }
    }
}