using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NorthwindIntl.ExceptionFilter
{
    public class CustomExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            var ex =context.Exception;
            System.Console.WriteLine(ex.Message);
            context.Result=new JsonResult(new {ExceptionFilterAttribute=ex.Message});

            // To Dont propagate the exception we should explicitly say it
            context.ExceptionHandled=true;
        }
    }
}