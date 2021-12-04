using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Src.Data.IdentityModels;

namespace Src.Controllers
{
    [Authorize]
    public class ClaimsController : Controller
    {
        private readonly ILogger<ClaimsController> _logger;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public ClaimsController(ILogger<ClaimsController> logger, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            return View(User?.Claims);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Required] string claimType, [Required] string claimValue)
        {
            if (ModelState.IsValid)
            {
                // Build Claim
                var claim = new Claim(claimType, claimValue, ClaimValueTypes.String);
                // get the identity user that we want to create claims for him/her
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    // add claim to user
                    var result = await _userManager.AddClaimAsync(user, claim);
                    if (result.Succeeded)
                    {
                        await _signInManager.RefreshSignInAsync(user);
                        return RedirectToAction("Index");
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "User is not Found");
                }

            }
            return View();

        }

        [HttpPost]
        public async Task<IActionResult> Delete(string _claim)
        {
            var claimFields = _claim.Split(";");
            if (claimFields.Length < 4)
            {
                ModelState.AddModelError("", "The claim detail is not complete");
                return View(User?.Claims);
            }
            string claimType = claimFields[0], claimValue = claimFields[1], claimValueType = claimFields[2], claimIssuer = claimFields[3];
            // var claim = new Claim(claimType, claimValue, claimValueType); // Since we do not any validation on form entries better to get claim from current claims
            var claim = User.Claims.Where(c => c.Type == claimType && c.Value == claimValue && c.ValueType == claimValueType && c.Issuer == claimIssuer).FirstOrDefault();
            if (claim != null)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var result = await _userManager.RemoveClaimAsync(user, claim);
                    if (result.Succeeded)
                    {
                        await _signInManager.RefreshSignInAsync(user);
                        return RedirectToAction("Index");
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "User Not Found");
                }

            }
            else
            {
                ModelState.AddModelError("", "Claim Not exist");
            }
            return View(User?.Claims);
        }

    }
}