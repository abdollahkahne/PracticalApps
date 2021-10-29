using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NorthwindIntl.ExceptionFilter
{
    public class AddHeaderWithFactoryAttribute : Attribute, IFilterFactory
    {
        public bool IsReusable => false;

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            // we can inject services from IServiceProvider to Filter Constructor
            return new InternalAddHeaderFilter();
        }

        private class InternalAddHeaderFilter:ResultFilterAttribute {
            public override void OnResultExecuting(ResultExecutingContext context) {
                context.HttpContext.Response.Headers.Add("Internal",new string[] {"header value"});
            }
            public override void OnResultExecuted(ResultExecutedContext context) {}
        } 
    }
}