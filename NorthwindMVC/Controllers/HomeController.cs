using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PracticalApp.NorthwindMVC.Models;
using PracticalApp.NorthwindContextLib;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using Newtonsoft.Json;
using PracticalApp.NorthwindEntitiesLib;

namespace PracticalApp.NorthwindMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private Northwind _db;
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(ILogger<HomeController> logger, Northwind db, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _db = db;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var model = new HomeIndexViewModel
            {
                VisitorCount = (new Random()).Next(1, 1001),
                Products = await _db.Products.ToArrayAsync(),
                Categories = await _db.Categories.ToArrayAsync(),
            };
            return View(model);
        }

        [Route("private")]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> ProductDetails(int? id)
        {
            if (!id.HasValue)
            {
                return NotFound("You must pass a product ID in the route, for example, /Home/ProductDetail/21");
            }

            var product = await _db.Products.SingleOrDefaultAsync(p => p.ProductID == id);
            if (product == null)
            {
                return NotFound($"Product with ID of {id} not found.");
            }
            return View(model: product);
        }
        public IActionResult ModelBinding()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ModelBinding([FromForm] Thing thing)
        {
            // Binding Order: 1- Form Value (in Property Level and not the object level) 2- Route Parameter 3- Query String Parameter
            // There is also other source of values like Headers, Request Body in case of non form data, 
            // return View(thing);
            foreach (var item in ModelState.Values)
            {
                Console.WriteLine(item.AttemptedValue);
                foreach (var err in item.Errors)
                {
                    Console.WriteLine(err.ErrorMessage);
                }
            }

            Console.WriteLine(ModelState.ValidationState);
            Console.WriteLine(ModelState.IsValid);
            var model = new HomeModelBindingViewModel
            {
                Thing = thing,
                HasError = !ModelState.IsValid,
                ValidationErrors = ModelState.Values
                .SelectMany(value => value.Errors)
                .Select(err => err.ErrorMessage),
            };
            return View(model);
        }
        public IActionResult ProductsThatCostsMoreThan(decimal? price)
        {
            if (!price.HasValue)
            {
                return NotFound("You must pass a product price in the query string, for example, /Home/ProductsThatCostMoreThan?price=50");
            }
            var model = _db.Products.Include(p => p.Category)
            .Include(p => p.Supplier).Where(p => p.UnitPrice > price).AsEnumerable();
            if (model.Count() == 0)
            {
                return NotFound($"No products cost more than {price:C}.");
            }
            ViewData["MinPrice"] = price.Value.ToString("C");
            return View(model);
        }

        public async Task<IActionResult> Customers(string country)
        {
            string endpoint;
            if (String.IsNullOrEmpty(country))
            {
                endpoint = "_api/customers";
                ViewData["Title"] = "Customers Around the World";
            }
            else
            {
                endpoint = $"_api/customers?country={country}";
                ViewData["Title"] = $"Customers in {country.ToUpper()}";
            }

            // Here we use one Factory for each base address but multiple instance with each thread (each visit)
            var client = _httpClientFactory.CreateClient("Northwind API");
            var request = new HttpRequestMessage(method: HttpMethod.Get, requestUri: endpoint);
            var response = await client.SendAsync(request);
            // var response = await client.GetAsync(endpoint);
            var jsonString = await response.Content.ReadAsStringAsync();

            var customers = JsonConvert.DeserializeObject<IEnumerable<Customer>>(jsonString);
            return View(customers);
        }
    }
}
