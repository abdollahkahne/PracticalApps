using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NorthwindIntl.ValueProviders
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Method|AttributeTargets.Class,AllowMultiple =false,Inherited =true)]
    public sealed class ValidateModelStateAttribute:ActionFilterAttribute
    {
        public ValidateModelStateAttribute(string redirectUrl)
        {
            RedirectUrl = redirectUrl;
        }

        public ValidateModelStateAttribute( string action, string controller=null, object routeData=null)
        {            
            Controller = controller;
            Action = action;
            RouteData = routeData;
        }

        public string RedirectUrl {get;}
        public string Controller {get;}
        public string Action {get;}
        public object RouteData {get;}

        public override Task OnResultExecutionAsync(ResultExecutingContext context,ResultExecutionDelegate next) {
            Console.WriteLine("I runned to validate top level nodes (Filter)");
            if (!context.ModelState.IsValid) {
                if (!string.IsNullOrEmpty(RedirectUrl)) {
                    context.Result=new RedirectResult(RedirectUrl);
                } else if (string.IsNullOrEmpty(Action)) {
                    context.Result=new RedirectToActionResult(Action,Controller,RouteData);
                } else {
                    context.Result=new BadRequestObjectResult(context.ModelState);
                }
            } 
            return base.OnResultExecutionAsync(context,next);

        }
        
    }
}