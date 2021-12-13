using System;
using System.Collections.Generic;
// using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Primitives;

namespace ObjectPoolSample.Middlewares
{
    public class BirthDayMiddleware
    {
        private readonly RequestDelegate _next;

        public BirthDayMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context, ObjectPool<StringBuilder> pool)
        {
            bool fNamePresented = context.Request.Query.TryGetValue("firstname", out StringValues fName);
            bool lNamePresented = context.Request.Query.TryGetValue("lastname", out var lName);
            bool monthPresented = context.Request.Query.TryGetValue("month", out var month);
            bool monthIsInt = int.TryParse(month, out int monthOfYear);
            bool dayIsPresented = context.Request.Query.TryGetValue("day", out var day);
            bool dayIsInt = int.TryParse(day, out int dayOfMonth);
            if (fNamePresented && lNamePresented && monthPresented && monthIsInt && dayIsPresented && dayIsInt)
            {
                var today = DateTime.UtcNow;
                // Request a string builder from pool
                var stringBuilder = pool.Get();// if the pool does not have an object it create one for us
                try
                {
                    stringBuilder.Append("Hi ").Append(fName).Append(" ").Append(lName).Append(".");
                    var encoder = context.RequestServices.GetRequiredService<HtmlEncoder>(); // Why we do not inject it in DI? service locator is anti pattern and should be avoided
                    if (today.Month == monthOfYear && today.Day == dayOfMonth)
                    {
                        stringBuilder.Append("Happy Birthday!");
                        var html = encoder.Encode(stringBuilder.ToString());
                        await context.Response.WriteAsync(html); // response should be text (serialized to text) and sent. 
                    }
                    else
                    {
                        var thisYearBirthday = new DateTime(today.Year, monthOfYear, dayOfMonth);
                        int daysUntilBirthday = (today < thisYearBirthday) ?
                        (thisYearBirthday - today).Days :
                         (thisYearBirthday.AddYears(1) - today).Days;
                        stringBuilder.Append("There are ").Append(daysUntilBirthday).Append("days until your birthday");
                        var html = encoder.Encode(stringBuilder.ToString());
                        await context.Response.WriteAsync(html);
                    }
                }
                catch (System.Exception)
                {

                    throw;
                }
                finally
                { // this runs always even in case of exception
                    pool.Return(stringBuilder);
                }
                return; // this makes that middleware be the last one in pipeline
            }
            await _next(context);
        }
    }
}