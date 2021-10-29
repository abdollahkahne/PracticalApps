using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace NorthwindIntl.ExceptionFilter
{
    [AttributeUsage(AttributeTargets.Method,Inherited =true,AllowMultiple =false)]
    public sealed class CacheResourceFilter : Attribute, IResourceFilter
    {
        public TimeSpan Duration {get;set;}

        public CacheResourceFilter(int duration)
        {
            Duration = TimeSpan.FromSeconds(duration);
        }

        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            var cacheKey=context.HttpContext.Request.Path.ToString().ToLowerInvariant();
            var memoryCache=context.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();
            var result=context.Result as ContentResult;

            // Cache it in case of non empty
            if (result!=null) {
                memoryCache.Set(cacheKey,result,Duration);
            }
        }

        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            var cacheKey=context.HttpContext.Request.Path.ToString().ToLowerInvariant();
            var memoryCache=context.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();
            var isCached=memoryCache.TryGetValue(cacheKey,out var result);
            if (isCached && result!=null && result is string stringResult) {
                context.Result=new ContentResult {
                    Content=stringResult
                };
            }
        }
    }
}