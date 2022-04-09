using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication;

namespace GoogleBlazor.Controllers
{
    public class AccountingController : Controller
    {
        private readonly ILogger<AccountingController> _logger;

        public AccountingController(ILogger<AccountingController> logger)
        {
            _logger = logger;
        }

        [Route("accounting/login")]
        public ActionResult Login()
        {

            return new ChallengeResult("Google", new AuthenticationProperties() { RedirectUri = Url.RouteUrl("GoogleUrl") });
        }
        [Route("signin-google", Name = "GoogleUrl")]
        public async Task<IActionResult> GoogleResponse()
        {
            // await HttpContext.SignInAsync(HttpContext.User);
            Console.WriteLine(await HttpContext.GetTokenAsync("Cookies", "access_token"));
            return Redirect("https://localhost:7125/");
        }

    }
}