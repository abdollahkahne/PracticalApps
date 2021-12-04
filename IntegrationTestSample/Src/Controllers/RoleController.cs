using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Src.Data.IdentityModels;

namespace Src.Controllers
{
    public class RoleController : Controller
    {
        private readonly ILogger<RoleController> _logger;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;

        public RoleController(ILogger<RoleController> logger, RoleManager<AppRole> roleManager, UserManager<AppUser> userManager)
        {
            _logger = logger;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var roles = _roleManager.Roles.AsEnumerable();
            return View(roles);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([Required] string name, string description)
        {
            if (ModelState.IsValid)
            {
                var result = await _roleManager.CreateAsync(new AppRole { Name = name, Description = description });
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));

                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View(new { Name = name, description = description });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role != null)
            {
                var result = await _roleManager.DeleteAsync(role);
                if (result.Succeeded)
                {
                    RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            else
            {
                ModelState.AddModelError("", "The role with specified Id does not Found");
            }
            return View("Index", _roleManager.Roles.AsEnumerable()); // Consider that the model of razor view always should be passed to view unless we use redirect to which passed it in its related action

        }

        public async Task<IActionResult> Update(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId); // If we want to use EF for easily finding user in role we should determine Join Entities in DbContext Definitions like UserRoles
            var members = new List<AppUser>();
            var noneMembers = new List<AppUser>();
            foreach (var user in _userManager.Users.AsEnumerable())
            {
                var list = (await _userManager.IsInRoleAsync(user, role?.Name)) ? members : noneMembers;
                list.Add(user);
            }
            return View(new RoleEdit
            {
                Role = role,
                Members = members,
                NoneMembers = noneMembers
            });
        }

        [HttpPost]
        public async Task<IActionResult> Update(RoleModification model)
        {
            var role = await _roleManager.FindByNameAsync(model.RoleName);
            if (role == null)
            {
                ModelState.AddModelError("", "Role name is not valid");
            }
            if (ModelState.IsValid)
            {
                foreach (var id in model.AddedIds ?? new string[] { })
                {
                    var appUser = await _userManager.FindByIdAsync(id);
                    if (appUser != null)
                    {
                        var result = await _userManager.AddToRoleAsync(appUser, model.RoleName);
                        if (!result.Succeeded)
                        {
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError("", error.Description);
                            }
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", $"User with {id} not found");
                    }

                }

                // Delete Selected Ids
                foreach (var id in model.DeletedIds ?? new string[] { })
                {
                    var appUser = await _userManager.FindByIdAsync(id);
                    if (appUser != null)
                    {
                        var result = await _userManager.RemoveFromRoleAsync(appUser, model.RoleName);
                        if (!result.Succeeded)
                        {
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError("", error.Description);
                            }
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", $"User with {id} not found");
                    }

                }
            }

            // If Model state remain valid redirect to Update Page otherwise remain in the view with errors
            if (ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }
            // Here we do not call MVC action, we only call the view but beforehand calculated the model (call a method)
            return await Update(role.Id);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}