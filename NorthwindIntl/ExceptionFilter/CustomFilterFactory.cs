using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using NorthwindIntl.Transformer;

namespace NorthwindIntl.ExceptionFilter
{
    // We can not use DI as constructor on Attributes like Filters altough we can use them from 
    // service Provider of HttpContext which passed by context to its On... methods.
    // The alternative is to use Facory since it has service provider too and send it to Filter Constructor
    public class CustomFilterFactory : IFilterFactory
    {
        public bool IsReusable => true;

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            // For example we need ITranslator service
            var service=serviceProvider.GetRequiredService<ITranslator>();
            return new CustomFilter(service);
        }
    }

    public class CustomFilter : ActionFilterAttribute
    {
        private readonly ITranslator _service;
        public CustomFilter(ITranslator service)
        {
            _service=service;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next) {
            var translated=await _service.Translate("en","Hello");
            Console.WriteLine(translated);
            await base.OnActionExecutionAsync(context,next);
        }
    }
}