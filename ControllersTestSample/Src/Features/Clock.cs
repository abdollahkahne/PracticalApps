using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Src.Features
{
    public class Clock
    {
        public DateTimeOffset DateTime { get; set; } = DateTimeOffset.UtcNow;
        public TimeZoneInfo TimeZone { get; set; } = TimeZoneInfo.Utc;
        public DateTimeOffset Local => TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime, TimeZone?.Id ?? TimeZoneInfo.Utc.Id);
    }

    // We should register Feature within a middleware
    // Since we use IMiddleware here, we need to register this in service collection. This gaves us following benefit: (Factory-based middleware vs Convention-based middleware)
    // uses strong typing and per-request activation
    // 1-  Middleware is constructed once per application lifetime in Convention-Based while in Factory-Based it activate Per-Request
    // 2- Both of two Approach can accept dependencies within its constructor on Invoke Async method parameters
    // 3- It isn't possible to pass objects to the factory-activated middleware with UseMiddleware. Also the InvokeAsync has only two parameters including context and next. All Injection should happen in constructor
    // 4- Because middleware is constructed at app startup, not per-request, scoped lifetime services used by middleware constructors aren't shared with other dependency-injected types during each request (Middleware has its own scope which is different from request scope and other types scope!). If you must share a scoped service between your middleware and other types, add these services to the Invoke method's signature. The Invoke method can accept additional parameters that are populated by DI
    // 5- In Factory-Based, the middleware should registered as a scoped or transient service in the app's service container. and it is activated per request so we can inject scoped services to them using constructor.
    public class ClockMiddleware : IMiddleware
    {
        public const string TimeZoneKey = "clock.timezone";
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var clock = new Clock();
            if (context.Request.Cookies.TryGetValue(TimeZoneKey, out var timezone))
            {
                clock.TimeZone = TimeZoneInfo.FindSystemTimeZoneById(timezone) ?? TimeZoneInfo.Utc;
            }
            context.Features.Set<Clock>(clock);
            await next(context);
        }

        public static void SetTimeZone(HttpResponse response, string timezoneId)
        {
            response.Cookies.Append(TimeZoneKey, timezoneId);
        }
    }
}

// Question To Ask Before making a feature:
// Is feature data generally stable or even read-only? Yes, a user’s time zone does not change frequently once selected.
// Is feature data used in many places throughout my request pipeline? Most likely. If I were building a data application, I might want to know a user’s time zone to convert application times to relevant user times appropriately.
// Is the feature data dependant on an HTTP element such as cookies, query strings, or headers? yes! We are storing the timezone in a cookie. Even if we weren’t, we’d most likely be using a user identifier from the current user.