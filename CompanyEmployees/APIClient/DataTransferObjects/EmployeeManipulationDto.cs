using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace APIClient.DataTransferObjects
{
    public abstract class EmployeeManipulationDto
    {
        [Required(ErrorMessage = "Employee name is a required field.")]
        [MaxLength(30, ErrorMessage = "Maximum length for the Name is 30 characters.")]

        public string Name { get; set; }

        [Required(ErrorMessage = "Age is a required field.")]
        [Range(1, int.MaxValue, ErrorMessage = "Age should be more than {1}")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Position is a required field.")]
        [MaxLength(20, ErrorMessage = "Maximum length for the Position is 20 characters.")]

        public string Position { get; set; }
    }
}