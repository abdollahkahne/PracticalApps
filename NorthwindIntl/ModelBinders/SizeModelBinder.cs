using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NorthwindIntl.Models;

namespace NorthwindIntl.ModelBinders
{
    public class SizeModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var model=bindingContext.ModelName;
            var value=bindingContext.ValueProvider.GetValue(model);
            if (value!=ValueProviderResult.None) {
                bindingContext.ModelState.SetModelValue(model,value);
                var val=value.FirstValue;
                if (!string.IsNullOrWhiteSpace(val)) {
                    if (Enum.TryParse<PaperSize>(val,out PaperSize size)) {
                        bindingContext.Result=ModelBindingResult.Success(size);
                    } else {
                        bindingContext.ModelState.TryAddModelError(model,"Invalid Book Size");
                    }
                    
                } 
            }
            return Task.CompletedTask;
        }
    }
}