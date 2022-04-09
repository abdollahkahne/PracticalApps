using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication;

namespace GoogleTest.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;

        public AccountController(ILogger<AccountController> logger)
        {
            _logger = logger;
        }

        [Route("account/login")]
        public ActionResult Login()
        {

            return Challenge(new AuthenticationProperties() { RedirectUri = Url.RouteUrl("GoogleUrl") }, "Google");
        }
        [Route("yahoo-google", Name = "GoogleUrl")]
        public async Task<IActionResult> GoogleResponse()
        {
            // await HttpContext.SignInAsync(HttpContext.User);
            // Console.WriteLine(await HttpContext.GetTokenAsync("Cookies", "access_token"));
            return Redirect("https://localhost:7125/");
        }

    }
}