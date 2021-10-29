using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Hosting;

namespace NorthwindIntl.ExceptionFilter
{
    public class CustomExceptionHandlerFilter:Attribute,IExceptionFilter
    {
        private readonly IModelMetadataProvider _provider;
        private readonly IWebHostEnvironment _environment;

        // we can not use ModelMetadataProvider in Constructor injection since it has not default value
        // Better to inject interfaces (which are registered)
        public CustomExceptionHandlerFilter(IModelMetadataProvider provider, IWebHostEnvironment environment)
        {
            _provider = provider;
            _environment = environment;
        }

        public void OnException(ExceptionContext context)
        {
            if (_environment.IsDevelopment()) {
                var result=new ViewResult {
                    ViewName="CustomExceptionHandler",
                };
                context.ExceptionHandled=true;
                result.ViewData=new ViewDataDictionary(_provider,context.ModelState);
                result.ViewData.Add("Exception",context.Exception);
                context.Result=result;
            } else {return;}
        }
    }
}