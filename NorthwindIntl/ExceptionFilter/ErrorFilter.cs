using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NorthwindIntl.ExceptionFilter
{
    public class ErrorFilter:ExceptionFilterAttribute
    {
        public override async Task OnExceptionAsync(ExceptionContext context)
        {
            context.ExceptionHandled=true;
            await context.HttpContext.Response.WriteAsync($"An error occured: {context.Exception.Message}");

        }
    }
}