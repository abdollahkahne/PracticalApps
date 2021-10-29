using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NorthwindIntl.ExceptionFilter
{
    // Controllers annotated with the [ApiController] attribute automatically validate model state and return a 400 response. 
    public class ValidateModelAttribute:ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context) {
            if (!context.ModelState.IsValid) {
                context.Result=new BadRequestObjectResult(context.ModelState);
            }
            // In the case of short-circuiting we should not return base.On... Or await next(context);
            
        }
        
    }
}