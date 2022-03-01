using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorWebAssemblySignalRApp.Client.Pages.Form
{
    public class BasicModel
    {
        public bool Feature1 { get; set; }
        public bool Feature2 { get; set; }
        [Required]
        [Range(minimum: 18, maximum: 100, ErrorMessage = "Sample Error Message")]
        public int? Age { get; set; }
        public DateTime? BirthDate { get; set; }
        public bool valid => Feature1 || Feature2;
    }
}