using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NorthwindIntl.ExceptionFilter
{
    public class ShortCircuting : Attribute,IResourceFilter
    {
        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            
        }

        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            // To Short Circuit we should use resource filter and set its context's result to sth
            context.Result=new ContentResult {
                Content="Short Circuiting happpened!",
            };
        }
    }
}