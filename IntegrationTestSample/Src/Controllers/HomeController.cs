using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Src.Data.IdentityModels;
using Src.Data.TagHelperModel;

namespace Src.Controllers
{
    public class HomeController : Controller
    {
        private IProductRepository _repository;
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<AppUser> _userManager;

        public HomeController(ILogger<HomeController> logger, UserManager<AppUser> userManager, IProductRepository repository)
        {
            _logger = logger;
            _userManager = userManager;
            _repository = repository;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            // How to get Id of User Manager? if it is a claim you can directly get it from HttpContext.User
            // Otherwise you need to use UserManager (You can check sign in status using SignInManager Or IsAuthenticated Property of ClaimsIdentity)
            // Note even for unautenticated case User is not null and User!=null is always true
            AppUser appUser = null;
            if (User.Identity.IsAuthenticated)
            {
                appUser = await _userManager.GetUserAsync(User);
            }

            return View(appUser);
        }

        public IActionResult Products()
        {
            return View(_repository.Products);
        }

        public IActionResult CreateProduct()
        {
            ViewBag.Quantites = new SelectList(_repository.Products.OrderBy(p => p.Quantity).Select(p => p.Quantity).Distinct());
            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult CreateProduct(Product newProduct)
        {
            _repository.AddProduct(newProduct);
            return RedirectToAction("Products");
        }

        public IActionResult EditProduct()
        {
            var product = _repository.Products.LastOrDefault();
            ViewBag.Quantites = new SelectList(_repository.Products.OrderBy(p => p.Quantity).Select(p => p.Quantity).Distinct());
            return View("CreateProduct", product);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}