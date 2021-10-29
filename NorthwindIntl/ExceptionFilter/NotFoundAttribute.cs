using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NorthwindIntl.ExceptionFilter
{
    public class NotFoundAttribute : Attribute,IAlwaysRunResultFilter
    {
        public void OnResultExecuted(ResultExecutedContext context)
        {
            // if (context.Result is ObjectResult result && result.Value==null) {
            //     context.Result=new NotFoundResult();
            // }
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.Result is ObjectResult result && result.Value==null) {
                // context.Result=new NotFoundResult();
                context.Result=new JsonResult(new {
                    X=1,
                    Y=1,
                    Z=10
                });
            }
        }
    }
}