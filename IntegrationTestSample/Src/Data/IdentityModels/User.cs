using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Src.Data.IdentityModels
{
    // This is used as view model for User Creation
    public class User
    {
        [Required]
        public string Name { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public Country Country { get; set; }
        public int Age { get; set; }
        [Required]
        public string Salary { get; set; }
    }
}