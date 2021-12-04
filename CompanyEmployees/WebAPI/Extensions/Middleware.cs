using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Contracts;
using Entities.ErrorModel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace WebAPI.Extensions
{
    public static class Middleware
    {
        public static void UseExtendedExceptionHandler(this IApplicationBuilder app, ILoggerManager logger)
        {
            // we use built in exception handler and extend it to send the re-executed request to new pipeline which made it here our self as Error App
            // when an Error happens, it captured using the The Exceptional Handler Feature and a new complete request dispatched with data related to the exception exist in the feature
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";
                    var exceptionHandlingFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (exceptionHandlingFeature != null)
                    {
                        logger.LogError($"Something went wrong {exceptionHandlingFeature.Error}");
                        await context.Response.WriteAsync((new ErrorDetails
                        {
                            StatusCode = context.Response.StatusCode,
                            Message = "Internal Server Error",
                        }).ToString());
                    }
                });
            });
        }
    }
}