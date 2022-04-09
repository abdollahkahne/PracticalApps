using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using SharpPad;

namespace BlazorServerSignalRApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;

        public AccountController(ILogger<AccountController> logger)
        {
            _logger = logger;
        }

        [HttpPost("/{Controller}/logout")]
        public async Task<ActionResult> Logout()
        {
            // Sign out of Google Open ID is not necessary since Maximum age of token is just one hour and so the token is not valid after that but you can use related endpoint for that
            // await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync("Cookies");
            // var x = new SignOutResult(new List<string> { OpenIdConnectDefaults.AuthenticationScheme, "Cookies" });
            return Redirect("/");

        }

        [Route("account/login")]
        public ActionResult Login()
        {

            return new ChallengeResult(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties() { RedirectUri = Url.RouteUrl("GoogleUrl") });
        }
        [Route("google-response", Name = "GoogleUrl")]
        public async Task<IActionResult> GoogleResponse()
        {
            await HttpContext.Request.Headers.Dump();
            await HttpContext.User.Identity.Dump("Identity");
            // await HttpContext.SignInAsync(HttpContext.User);
            // Console.WriteLine(@"Access Token: {0}", await HttpContext.GetTokenAsync("Cookies", "access_token"));
            // Console.WriteLine(@"ID Token: {0}", await HttpContext.GetTokenAsync("Cookies", "id_token"));
            return Redirect("https://localhost:7125/");
            // return Content("Check Cookies and other info of request");
        }

    }
}