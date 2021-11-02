using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Src.Models
{
    public class NewSession
    {
        [Required]
        public string SessionName { get; set; }
    }
}