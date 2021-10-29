using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NorthwindIntl.Controllers
{
    public class AccountController:Controller
    {

        [AllowAnonymous]
        public IActionResult Login() {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> PerformLogin(string username,string password,string returnUrl,bool isPersistent) {
            if (await IsValidCredentials(username,password,isPersistent)) {
                var claims=new [] {
                        new Claim(ClaimTypes.Name,username),
                        new Claim(ClaimTypes.Role,"admin"),
                };
                var identity=new ClaimsIdentity(claims,CookieAuthenticationDefaults.AuthenticationScheme);
                var user=new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,user,new AuthenticationProperties {IsPersistent=isPersistent});
                //identity
                //await this._signInManager.SignInAsync(user, new AuthenticationProperties { IsPersistent = isPersistent });
                return LocalRedirect(returnUrl);
            } else {
                return RedirectToAction(nameof(UnAuthenticated));
            }
        }

        public async Task<IActionResult> Logout() {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            //identity
            //await this._signInManager.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index),"Home");
        }

        [AllowAnonymous]
        public IActionResult UnAuthenticated()
        {
            return View();
        }

         private async Task<bool> IsValidCredentials(string username, string password, bool isPersistent)
        {
            //dummy
            return true;
            //identity
            //var result = await _signInManager.PasswordSignInAsync(username, password, isPersistent, false);
            //return result.Succeeded;
        }
    }
}