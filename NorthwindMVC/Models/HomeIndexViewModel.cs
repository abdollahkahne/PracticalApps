using System.Collections.Generic;
using PracticalApp.NorthwindEntitiesLib;

namespace PracticalApp.NorthwindMVC.Models
{
    public class HomeIndexViewModel
    {
        public int VisitorCount;
        public IEnumerable<Product> Products { get; set; }
        public IEnumerable<Category> Categories { get; set; }
    }
}