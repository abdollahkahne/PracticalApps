using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PracticalApp.NorthwindAPI.Utilities;

namespace PracticalApp.NorthwindAPI.Controllers
{
    public class CFG {
        public string AllowedHosts {get;set;}
    }

    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [ApiVersion("2.0")]
    [ApiVersion("1.0")]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IConfigurationRoot _configuration;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration as IConfigurationRoot;
        }

        // Get /weatherforecast
        [Authorize]
        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [MapToApiVersion("2.0")]
        public IEnumerable<WeatherForecast> Get()
        {
            // Console.WriteLine(_configuration["AllowedHosts"]);
            // var cfg=_configuration.Get<CFG>();
            // Console.WriteLine(cfg.AllowedHosts);
            var config1=AppContext.GetData("Config1");
            Console.WriteLine(config1);
            return Get(3);
        }

         // Get /weatherforecast
        [Authorize]
        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [MapToApiVersion("1.0")]
        public IEnumerable<WeatherForecast> GetV1()
        {
            // Console.WriteLine(_configuration["AllowedHosts"]);
            // var cfg=_configuration.Get<CFG>();
            // Console.WriteLine(cfg.AllowedHosts);
            var config1=AppContext.GetData("Config1");
            Console.WriteLine(config1);
            return Get(5);
        }

        // Get /weatherforecast/7
        [Authorize]
        [HttpGet("{days:int}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public IEnumerable<WeatherForecast> Get(int days)
        {
            var rng = new Random();
            return Enumerable.Range(1, days).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet]
        [Route("Token")]
        [ProducesResponseType(200)]
        public JsonResult GetAuthenticationToken() {
            var jwt=AuthenticationExtensions.SignJWTToken("ardeshir");
            return new JsonResult(jwt);
        }
    }
}
