using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace NorthwindIntl.ValueProviders
{
    public class HelloWorldActionResult : IActionResult
    {
        public async Task ExecuteResultAsync(ActionContext context)
        {
            // status codes
            context.HttpContext.Response.StatusCode=StatusCodes.Status200OK;
            //Content Type
            //Content
            await context.HttpContext.Response.WriteAsync("Hello Action Results!");
        }
    }
}