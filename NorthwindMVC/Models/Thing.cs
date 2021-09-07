using System.ComponentModel.DataAnnotations;

namespace PracticalApp.NorthwindMVC.Models
{
    public class Thing
    {
        [Range(1, 10)]
        public int? Id { get; set; }
        [Required]
        public string Color { get; set; }
    }
}