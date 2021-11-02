using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Src.Core.Interfaces;
using Src.Core.Models;
using Src.Models;

namespace Src.Controllers
{
    public class HomeController : Controller
    {
        private readonly IBrainstormSessionRepository _repository;


        public HomeController(IBrainstormSessionRepository repository)
        {
            _repository = repository;
        }

        public async Task<IActionResult> Index()
        {
            var sessions = await _repository.ListAsync();
            var model = sessions.Select(session => new StormSessionViewModel
            {
                Id = session.Id,
                Name = session.Name,
                DateCreated = session.DateCreated,
                IdeaCount = session.Ideas.Count
            });
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(NewSession session)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            await _repository.AddAsync(new BrainstormSession
            {
                Name = session.SessionName,
                DateCreated = DateTimeOffset.Now,
            });
            return RedirectToAction(nameof(Index));
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
