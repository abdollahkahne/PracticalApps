using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PracticalApp.NorthwindAPI.Repositories;
using PracticalApp.NorthwindEntitiesLib;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace PracticalApp.NorthwindAPI.Controllers
{
    // base address: /_api/customers
    [ApiController]
    [Route("_api/[Controller]")]
    public class CustomersController : ControllerBase
    {
        private ICustomerRepository _repository;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(ICustomerRepository repository, ILogger<CustomersController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        // Get: _api/customers
        // Get: _api/customers?country=[country]
        // this will always return a list of customers even if its empty
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Customer>))]
        public async Task<IEnumerable<Customer>> getCustomers(string country)
        {
            if (string.IsNullOrEmpty(country))
            {
                return await _repository.RetrieveAllAsync();
            }
            return (await _repository.RetrieveAllAsync()).Where(c => (c.Country.ToLower() == country.ToLower()));
        }

        // Get: _api/customers/[id]
        [HttpGet("{id}", Name = nameof(getCustomer))]
        [ProducesResponseType(200, Type = typeof(Customer))]
        [ProducesResponseType(404)]
        public async Task<IActionResult> getCustomer(string id)
        {
            var customer = await _repository.RetrieveAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return Ok(customer);
        }

        // Post: _api/customers
        // Body: Customer (JSON,XML)
        [HttpPost]
        [ProducesResponseType(200, Type = typeof(Customer))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] Customer customer)
        {
            if (customer == null)
            {
                return BadRequest();
            }
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var c = await _repository.CreateAsync(customer);
            // return Ok(c);
            // To use this we should name the related route
            return CreatedAtRoute(
                routeName: nameof(getCustomer),
                value: c,
                routeValues: new { id = c.CustomerID.ToLower() }
            );
        }

        // Put:_api/customers/[id]
        // Body: Customer(JSON,XML)
        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(string id, [FromBody] Customer customer)
        {
            id = id.ToUpper();
            if (customer == null || !ModelState.IsValid)
            {
                return BadRequest();
            }

            var exists = await _repository.RetrieveAsync(id);
            if (exists == null)
            {
                return NotFound();
            }

            var c = await _repository.UpdateAsync(id, customer);
            return NoContent();
        }

        // DELETE: _api/customers/[id]
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(string id)
        {
            //sample of a Custome Problem Details in 4xx status code
            if (id.ToUpper() == "BAD")
            {
                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Type = "https://localhost:5001/customers/failed-to-delete",
                    Title = $"Customer ID {id} found but failed to delete.",
                    Detail = "More details like Company Name, Country and so on.",
                    Instance = HttpContext.Request.Path,
                };
                return BadRequest(problemDetails);
            }
            var existing = await _repository.RetrieveAsync(id);
            if (existing == null)
            {
                return NotFound(); // 404 Resource not found
            }
            bool? deleted = await _repository.DeleteAsync(id);
            if (deleted.HasValue && deleted.Value) // short circuit AND
            {
                return new NoContentResult(); // 204 No content
            }
            else
            {
                return BadRequest( // 400 Bad request
                $"Customer {id} was found but failed to delete.");
            }
        }
    }
}