using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Src.Data.IdentityModels;

namespace Src.Controllers
{
    [Authorize]
    public class AuthenticateController : Controller
    {
        // The UserManager is used to manage Users in Identity
        // while the SignInManager is used to perform the authentication of the users.
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AuthenticateController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [AllowAnonymous]
        public IActionResult Login(string returnUrl)
        {
            if (!User.Identity.IsAuthenticated) // User is not null
            {
                Console.WriteLine("User is not Authenticated");
            }
            else
            {
                Console.WriteLine($"{User.GetType()}");
            }
            var login = new Login();
            login.ReturnUrl = returnUrl;
            return View(login);
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(Login login)
        {

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(login.UserName);
                if (user != null)
                {
                    // logout the session if already it is the case 
                    await _signInManager.SignOutAsync();

                    // Do sign In (including checking the password)
                    var result = await _signInManager.PasswordSignInAsync(user, login.Password, false, false);
                    if (result.Succeeded)
                    {
                        return Redirect(login.ReturnUrl ?? "/");
                    }
                    ModelState.AddModelError(nameof(login.UserName), "Login Failed: Invalid username or password");
                }
            }
            return View(login);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Redirect("/Index");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}