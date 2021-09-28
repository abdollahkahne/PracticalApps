using Microsoft.AspNetCore.Mvc;
using PracticalApp.NorthwindContextLib;
using System.Linq;

namespace PracticalApp.NorthwindMVC.ViewComponents
{
    public class SupplierViewComponent : ViewComponent
    {
        private readonly Northwind _db;

        public SupplierViewComponent(Northwind db)
        {
            _db = db;
        }
        public IViewComponentResult Invoke()
        {
            var suppliers = _db.Suppliers.ToArray();
            return View(viewName: "_SupplierViewComponent", model: suppliers);
        }
    }
}