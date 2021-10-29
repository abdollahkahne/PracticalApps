using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using NorthwindCookieAuth.Filters;

namespace NorthwindCookieAuth.Pages
{
    [Logger("This is a simple page model without Model State")]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            using (var scope=_logger.BeginScope(HttpContext.Connection.Id))
            {
                 var ctx=PageContext.ActionDescriptor.DisplayName;
            // Use names for the placeholders, not numbers. altough their sequence is importnat and not their name.
            // arguments themselves are passed to the logging system, not just the formatted message template.
            _logger.Log(LogLevel.Information,"Action Name is {ctx}",ctx);
            _logger.LogInformation("This is an Information: {ctx}",ctx);
            _logger.LogTrace("This is a Trace: {ctx}",ctx);
            // System.Diagnostics.Debug.WriteLine("This is writed in Debug window"); // Do not work in dotnet run

            // If the default log level is not set in Configuration, the default log level value is Information (Every log above it displayed).

            }
            
        }
    }
}
