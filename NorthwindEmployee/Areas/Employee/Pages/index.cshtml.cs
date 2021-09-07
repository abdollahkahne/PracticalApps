using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PracticalApp.NorthwindContextLib;
using PracticalApp.NorthwindEntitiesLib;

namespace PracticalApp.NorthwindEmployee.EmployeeFeature.Pages
{
    public class EmployeesModel : PageModel
    {
        private Northwind _db;
        public IEnumerable<Employee> Employees;
        public EmployeesModel(Northwind db)
        {
            _db = db;
        }

        public void OnGet()
        {
            Employees = _db.Employees.ToArray();
            ViewData["Title"] = "Employees List";
        }
    }
}
