using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SignalRSample.Models;
using SignalRSample.SignalR;

namespace SignalRSample.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHubContext<ChatHub> _hubContext; // This service should have a T which is a Hub
        // od course we have IHubContext<TypedHub,MyType> to simplify using method SendAsync of clients

        public HomeController(ILogger<HomeController> logger, IHubContext<ChatHub> hubContext, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _hubContext = hubContext;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                _hubContext.Clients.All.SendAsync("Alert", new { Message = $"Some body Called Index Controller of Home!:{User?.Identity?.Name}" });
                // _hubContext.Clients.All.SendAsync("Alert", ControllerContext); // This make to connection close with this error: Serialization and deserialization of 'System.Type' instances are not supported and should be avoided since they can lead to security issues
                _hubContext.Clients.All.SendAsync("Alert", new
                {
                    Message = new { UserId = _userManager.GetUserId(User), Name = "Ahmad", Age = 55, Married = true, Children = new[] { new { Name = "Mahmood", Age = 25, Married = false }, new { Name = "Reyhane", Age = 22, Married = false }, new { Name = "Parvane", Age = 27, Married = true } } }
                });
                // foreach (var item in User.Claims)
                // {
                //     Console.WriteLine(@"{0}:{1}", item.Type, item.Value);
                // }
                _hubContext.Clients.User(_userManager.GetUserId(User)).SendAsync("Alert", new { Message = "You connected to Home Controller!" });// user Id in SignalR Context is unique to nameidentifier Claim.
                _hubContext.Clients.User(User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value).SendAsync("Alert", new { Message = "You connected to Home Controller Using Name Identifier Claim!" });

            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
