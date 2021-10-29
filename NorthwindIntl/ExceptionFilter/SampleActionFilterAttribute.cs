using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace NorthwindIntl.ExceptionFilter
{
    // We can inherit from TypeFilter to inject service to Filter
    // Type Filter implemented Filter Factory so we can inject services to constructor from DI (but it is not for giving Parameters to it)
    public class SampleActionFilterAttribute : TypeFilterAttribute
{
    // we should give the filter type to base constructor
    public SampleActionFilterAttribute()
                         :base(typeof(SampleActionFilterImpl)) 
    { 
    }

    private class SampleActionFilterImpl : IActionFilter
    {
        private readonly ILogger _logger;
        public SampleActionFilterImpl(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<SampleActionFilterAttribute>();
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
           _logger.LogInformation("SampleActionFilterAttribute.OnActionExecuting");
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            _logger.LogInformation("SampleActionFilterAttribute.OnActionExecuted");
        }
    }
}
}