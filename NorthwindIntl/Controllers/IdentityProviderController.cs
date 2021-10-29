using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NorthwindIntl.Areas.Identity.Data;

namespace NorthwindIntl.Controllers
{
    public class IdentityProviderController:Controller
    {
        private readonly IOptions<CookieAuthenticationOptions> _options;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signinManager;

        public IdentityProviderController(IOptions<CookieAuthenticationOptions> options, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<ApplicationUser> signinManager)
        {
            _options = options;
            _userManager = userManager;
            _roleManager = roleManager;
            _signinManager = signinManager;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> PerformLogin(string username,string password,string returnUrl) {
            returnUrl??=Url.Content("~/");
            var result=await _signinManager.PasswordSignInAsync(username,password,isPersistent:false,lockoutOnFailure:false);
            if (result.Succeeded) {
                return LocalRedirect(returnUrl);
            } else if (result.IsLockedOut) {
                ModelState.AddModelError("UserName","User is locked out");
                return View("Login");
            }
            return Redirect(_options.Value.AccessDeniedPath.Value);
        }

        public async Task<IActionResult> Logout() {
            await _signinManager.SignOutAsync();
            return Redirect("~/");
        }

        private async Task<ApplicationUser> GetApplicationUserAsync() {
            var user=await _userManager.GetUserAsync(User);
            return user;
        }

        private async Task<IdentityRole> GetIdentityRoleAsync(string id) {
            return await _roleManager.FindByIdAsync(id);
        }
    }
}