using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BlazorWebAssemblySignalRApp.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace BlazorWebAssemblySignalRApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentsController : ControllerBase
    {
        private readonly ILogger<StudentsController> _logger;

        public StudentsController(ILogger<StudentsController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public JsonResult Index()
        {
            return new JsonResult(new
            {
                test = "Successful",
                date = DateTime.Now,
            });
        }

        [HttpPost]
        public IActionResult RegisterStudent(Student student)
        {
            // In case of Api: When a model binding validation error occurs on the server, an ApiController (ApiControllerAttribute) normally returns a default bad request response with a ValidationProblemDetails. 
            // So It seems that checking validity of model state is not required if you have [ApiController] attribute
            if (ModelState.IsValid)
            {
                try
                {
                    if (student.Country == Country.China && string.IsNullOrEmpty(student.Description))
                    {
                        ModelState.AddModelError(nameof(student.Description), $"{nameof(student.Description)} is required for students with country of origin {student.Country.ToString()}");
                        ModelState.AddModelError(string.Empty, $"Model Error: {nameof(student.Description)} is required for students with country of origin {student.Country.GetType().GetField(student.Country.ToString())!.GetCustomAttribute<DisplayAttribute>()!.GetName()}");
                    }
                    else
                    {
                        _logger.LogInformation("Processing the form asynchronously");
                        return Ok(ModelState);
                    }

                }
                catch (System.Exception ex)
                {

                    _logger.LogError("Validation Error: {Message}", ex.Message);
                }
            }

            return BadRequest(ModelState);
        }
    }
}