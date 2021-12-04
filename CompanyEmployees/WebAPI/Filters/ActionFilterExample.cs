using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebAPI.Filters
{
    public class ActionFilterExample : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            // This code should runned after action executed
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            // This code runned right before action method
        }
    }
}