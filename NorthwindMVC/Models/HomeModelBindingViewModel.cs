using System.Collections.Generic;

namespace PracticalApp.NorthwindMVC.Models
{
    public class HomeModelBindingViewModel
    {
        public Thing Thing { get; set; }
        public bool HasError { get; set; }
        public IEnumerable<string> ValidationErrors { get; set; }
    }
}