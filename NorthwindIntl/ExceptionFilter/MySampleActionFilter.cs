using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NorthwindIntl.ExceptionFilter
{
    public class MySampleActionFilter : Attribute,IActionFilter 
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Do something before the action executes.
        Console.WriteLine(MethodBase.GetCurrentMethod().ToString()+ context.HttpContext.Request.Path);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Do something after the action executes.
        Console.WriteLine(MethodBase.GetCurrentMethod().ToString()+ context.HttpContext.Request.Path);
    }
}
}