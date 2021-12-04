using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Src.Pages
{
    public class SecurePageModel : PageModel
    {
        public void OnGet()
        {
            Response.Headers.Add("username", User.Identity.Name);
        }
    }
}