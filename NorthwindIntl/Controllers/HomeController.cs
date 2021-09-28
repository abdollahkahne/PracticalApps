using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using NorthwindIntl.Models;
using NorthwindIntl.ValueProviders;

namespace NorthwindIntl.Controllers
{
    
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IStringLocalizer<HomeController> _localizer;

        public HomeController(ILogger<HomeController> logger, IStringLocalizer<HomeController> localizer)
        {
            _logger = logger;
            _localizer = localizer;
        }

        [MiddlewareFilter(typeof(LocalizationPipeline))]
        public IActionResult Translate() {
            Console.WriteLine(CultureInfo.CurrentCulture);
            var hello=_localizer["Hello"];
            var bye=_localizer["Bye"];
            return Json(new {
                Hello=hello.Value,
                Bye=bye.Value,
            });
        }


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult Submit(IFormCollection data) {
            return View();
        }
    }
}
