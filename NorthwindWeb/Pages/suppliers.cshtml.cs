using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PracticalApp.NorthwindContextLib;
using System.Linq;
using PracticalApp.NorthwindEntitiesLib;

namespace PracticalApp.NorthwindWeb
{
    public class SuppliersModel : PageModel
    {
        private Northwind _db;
        [BindProperty]
        public Supplier Supplier { get; set; }
        public IEnumerable<string> Suppliers { get; set; }
        public SuppliersModel(Northwind db)
        {
            _db = db;
        }
        public void OnGet()
        {
            ViewData["Title"] = "Suppliers";
            Suppliers = _db.Suppliers.Select(s => s.CompanyName);
        }

        public IActionResult OnPost()
        {
            if (ModelState.IsValid)
            {
                _db.Suppliers.Add(Supplier);
                _db.SaveChanges();
                return RedirectToPage("/suppliers");
            }
            return Page();
        }
    }
}