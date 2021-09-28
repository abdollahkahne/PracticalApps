using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Text.Encodings.Web;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Logging;

namespace NorthwindIntl.ValueProviders
{
    public class HTMLEncodeModelBinder : IModelBinder
    {
        private readonly IModelBinder _fallBackBinder=null;

        public HTMLEncodeModelBinder(IModelBinder fallBackBinder)
        {
            if (fallBackBinder==null) {
                throw new ArgumentNullException(nameof(fallBackBinder));
            }
            _fallBackBinder = fallBackBinder;
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            Console.WriteLine("Start");;
            if (bindingContext==null) {
                throw new ArgumentNullException(nameof(bindingContext));
            }
            var valueProviderResult=bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProviderResult==ValueProviderResult.None) {
                Console.WriteLine("result1");
                return _fallBackBinder.BindModelAsync(bindingContext);
            }
            var valueAsString=valueProviderResult.FirstValue;
            if (string.IsNullOrEmpty(valueAsString)) {
                Console.WriteLine("result2");
                return _fallBackBinder.BindModelAsync(bindingContext);
            }
            var result=HtmlEncoder.Default.Encode(valueAsString);
            Console.WriteLine(result);
            bindingContext.Result=ModelBindingResult.Success(result);
            return Task.CompletedTask;
        }
    }

    //This is used only in case of adding HTML encode Model Binder to Global list at startup
    public class HTMLEncodeModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context==null) {
                throw new NullReferenceException(nameof(context));
            }

            Console.WriteLine(context.BindingInfo.BinderType);
            if (context.Metadata.ModelType==typeof(string)) {
                var loggerFactory = context.Services.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
                return new HTMLEncodeModelBinder(new SimpleTypeModelBinder(context.Metadata.ModelType,loggerFactory));
            }
            return null;
        }
    }

    // this is only used if you want to use model binder provider at global list

    public class CustomHtmlEncodeAttribute:Attribute
    {
    }
}