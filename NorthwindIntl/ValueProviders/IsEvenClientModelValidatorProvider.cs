using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace NorthwindIntl.ValueProviders
{
    public class IsEvenClientModelValidatorProvider : IClientModelValidatorProvider
    {
        public void CreateValidators(ClientValidatorProviderContext context)
        {
            var type=context.ModelMetadata.ModelType;
            if (type==typeof(string) ||
                type==typeof(int) || type==typeof(uint) ||
                type==typeof(short) || type==typeof(ushort) ||
                type==typeof(long) || type==typeof(ulong) ||
                type==typeof(float) || type==typeof(double)
            ) {
                if (context.ValidatorMetadata.OfType<IsEvenAttribute>().Any()) {
                    if (!context.Results.Any(v =>v.Validator is IsEvenClientModelValidator)) {
                    context.Results.Add(new ClientValidatorItem {
                        Validator=new IsEvenClientModelValidator(),
                        IsReusable=true,
                    });
                }
                }
            }
        }
    }

    public class IsEvenClientModelValidator : IClientModelValidator
    {
        public void AddValidation(ClientModelValidationContext context)
        {
            context.Attributes["data-val"]=true.ToString().ToLowerInvariant();
            var validationAttribute =context.ModelMetadata.ValidatorMetadata.OfType<IsEvenAttribute>().SingleOrDefault();
            context.Attributes["data-val-iseven"]=validationAttribute.ErrorMessage;
        }
    }
}