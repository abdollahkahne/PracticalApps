using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Src.Core.Interfaces;
using Src.Models;

namespace Src.Controllers
{
    public class SessionController : Controller
    {
        private readonly IBrainstormSessionRepository _repository;

        public SessionController(IBrainstormSessionRepository repository)
        {
            _repository = repository;
        }

        public async Task<IActionResult> Index(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction(controllerName: "Home", actionName: nameof(Index));
            }
            var session = await _repository.GetByIdAsync(id.Value);
            if (session == null)
            {
                return Content("Session not found.");
            }
            var model = new StormSessionViewModel
            {
                Id = session.Id,
                IdeaCount = session.Ideas.Count,
                DateCreated = session.DateCreated,
                Name = session.Name,
            };
            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}