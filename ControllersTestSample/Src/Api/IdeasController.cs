using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Src.ClientModels;
using Src.Core.Interfaces;
using Src.Core.Models;

namespace Src.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class IdeasController : ControllerBase
    {
        private readonly IBrainstormSessionRepository _repository;

        public IdeasController(IBrainstormSessionRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("forsession/{sessionId}")]
        [ProducesResponseType(404)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> ForSession(int sessionId)
        {
            var session = await _repository.GetByIdAsync(sessionId);
            if (session == null)
            {
                return NotFound(sessionId);
            }
            var result = session.Ideas.Select(idea => new IdeaDTO
            {
                Id = idea.Id,
                Name = idea.Name,
                Description = idea.Description,
                DateCreated = idea.DateCreated,
            }).ToList();
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] NewIdeaModel newIdea)
        {
            // Console.WriteLine(@"Session Id:{0}", newIdea.SessionId);
            if (!ModelState.IsValid)
            {
                return BadRequest((ModelState));
            }
            var session = await _repository.GetByIdAsync(newIdea.SessionId);
            if (session == null) { return NotFound(newIdea.SessionId); };
            var idea = new Idea
            {
                Name = newIdea.Name,
                Description = newIdea.Description,
                DateCreated = DateTimeOffset.Now,
            };
            session.Ideas.Add(idea);
            await _repository.UpdateAsync(session);
            return Ok();
        }

    }
}