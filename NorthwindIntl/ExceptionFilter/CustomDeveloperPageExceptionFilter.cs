using System;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace NorthwindIntl.ExceptionFilter
{
    public class CustomDeveloperPageExceptionFilter : IDeveloperPageExceptionFilter
    {
        public async Task HandleExceptionAsync(ErrorContext errorContext, Func<ErrorContext, Task> next)
        {
            if (errorContext.Exception is DbException) {
                await errorContext.HttpContext.Response.WriteAsync("Error Connecting to Database!");
                await next(errorContext);
            } else {
                await errorContext.HttpContext.Response.WriteAsync("Error is not Connecting to Database!");
            }
        }
    }
}