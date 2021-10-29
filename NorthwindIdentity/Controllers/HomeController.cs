using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NorthwindIdentity.AuthorizationHandler;
using NorthwindIdentity.Models;

namespace NorthwindIdentity.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAuthorizationService _authSvc;

        public HomeController(ILogger<HomeController> logger, IAuthorizationService authSvc)
        {
            _logger = logger;
            _authSvc = authSvc;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            // Using authorization Pipeline for Business Validations
            var authResult=await _authSvc.AuthorizeAsync(user:User,resource:DateTime.Today.DayOfWeek,new DayOfWeekAuthorizationRequirement(DayOfWeek.Monday));
            // Console.WriteLine(@"Today is {0}",DateTime.Today.DayOfWeek);
            Console.WriteLine(@"Today is Monday: {0}",authResult.Succeeded);

            // Use Authorization Handler using Policy instead of Requirement Directly
            authResult=await _authSvc.AuthorizeAsync(
                user:User,
                resource:DateTime.Today.DayOfWeek,
                policyName:"IsMonday"
            );
            Console.WriteLine(@"Today is Monday: {0} (Checked By Policy)",authResult.Succeeded);

            // Here the constructor added by compiler
            // Unless the class is static, classes without constructors are given 
            // a public parameterless constructor by the C# compiler in order to enable class instantiation.
            authResult=await _authSvc.AuthorizeAsync(
                User,
                null,
                new LocalIPAuthorizationRequirement()
            );
            Console.WriteLine(@"Connection is Local: {0}",authResult.Succeeded);

            return View();
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
