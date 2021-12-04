using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Src.Data.IdentityModels;

namespace Src.Controllers
{
    public class AdminController : Controller
    {
        private UserManager<AppUser> _userManager;
        public AdminController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var users = _userManager.Users.AsEnumerable();
            return View(users);
        }

        public ViewResult Create() { return View(); }
        [HttpPost]
        public async Task<IActionResult> Create(User user)
        {
            if (ModelState.IsValid)
            {
                var appUser = new AppUser
                {
                    UserName = user.Name,
                    Email = user.Email,
                    Country = user.Country,
                    Age = user.Age,
                    Salary = user.Salary,
                };

                var result = await _userManager.CreateAsync(appUser, user.Password);
                if (result.Succeeded)
                {
                    // add Claims to user?
                    // var claim = new Claim("Age", user.Age.ToString(), ClaimValueTypes.Integer);
                    // await _userManager.AddClaimAsync(appUser, claim);

                    return RedirectToPage("/Index");
                }
                else
                {
                    // ModelState.AddModelError("", "Something is wrong");
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View(model: user);

        }

        public async Task<IActionResult> Update(string id)
        {
            var appUser = await _userManager.FindByIdAsync(id);
            if (appUser == null)
            {
                return RedirectToAction("Index");
            }
            var user = new User
            {
                Name = appUser.NormalizedUserName,
                Email = appUser.Email,
                Password = "Password should placed here!",
                Country = appUser.Country,
                Age = appUser.Age,
                Salary = appUser.Salary,
            };
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Update(string id, [FromForm] User user)
        {
            // This is not a secure endpoint since every one can easily find some one elses id
            // And then change their password for example. For security this is only open for user itself
            // var name=User.Identity.Name; for example by getting him using its Name of Id if exist as Claim!
            var appUser = await _userManager.FindByIdAsync(id);
            if (appUser == null)
            {
                ModelState.AddModelError("", "User wasn't found!");
                return View(user);
            }
            if (ModelState.IsValid)
            {
                // since password is hashed password validator does not apply to it. we should use other method for password or check the validators diectly
                var passwordValidationErrors = new List<IdentityError>();
                foreach (var passwordValidator in _userManager.PasswordValidators)
                {
                    var res = await passwordValidator.ValidateAsync(_userManager, appUser, user.Password);
                    if (!res.Succeeded)
                    {
                        passwordValidationErrors.AddRange(res.Errors);
                    }
                }
                if (passwordValidationErrors.Count == 0)
                {
                    // IPasswordHasher<AppUser> is used for hashing password. It can be injected from services or get from UserManager<AppUser>
                    appUser.PasswordHash = _userManager.PasswordHasher.HashPassword(appUser, user.Password);
                    appUser.Email = user.Email;
                    // appUser.UserName = user.Name;// Does this possible? Yes if it is already free
                    appUser.Country = user.Country;// This is wrong for Enum Types since it returns the name instead of value. It will be Okay if we correctly set the SelectList

                    appUser.Age = user.Age;
                    appUser.Salary = user.Salary;

                    var result = await _userManager.UpdateAsync(appUser);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                    }
                }
                else
                {
                    foreach (var error in passwordValidationErrors)
                    {
                        ModelState.AddModelError("Password", error.Description);
                    }
                }

            }
            return View(user);


        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            else
            {
                ModelState.AddModelError("", "User with specified Id does not find");
            }
            return View("Index", _userManager.Users.AsEnumerable());// we should pass the Model data too since we want to show Model State Errors


        }
    }
}