using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using PracticalApp.NorthwindContextLib;
using PracticalApp.NorthwindEntitiesLib;

namespace PracticalApp.NorthwindAPI.Controllers
{
    public class CustomersOdataController:ODataController
    {
       private readonly Northwind _context;

        public CustomersOdataController(Northwind context)
        {
            _context = context;
        }


        public IQueryable<Customer> Get()=>_context.Customers;
        
        public async Task<Customer> Get(string id)=>await _context.Customers.FindAsync(id);
    }
}