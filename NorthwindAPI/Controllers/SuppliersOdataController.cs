using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using PracticalApp.NorthwindContextLib;
using PracticalApp.NorthwindEntitiesLib;

namespace PracticalApp.NorthwindAPI.Controllers
{
    public class SuppliersController:ODataController
    {
       private readonly Northwind _context;

        public SuppliersController(Northwind context)
        {
            _context = context;
        }

        [EnableQuery]
        public IQueryable<Supplier> Get()=>_context.Suppliers;
        
        public async Task<Supplier> Get(int id)=>await _context.Suppliers.FindAsync(id);
    }
}