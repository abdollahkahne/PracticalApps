using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NorthwindCookieAuth.Authorization;

namespace PracticalApp.NorthwindCookieAuth.Northwind
{
    [MinimumAgeAuthorize(22)]
    public class IndexPageModel:PageModel
    {
        // 1- dependecy injection (private fields and ctor)
        private readonly IAuthorizationService _auth;
        public IndexPageModel(IAuthorizationService auth)
        {
            _auth = auth;
        }

        // 2- Define Input Models and Bind it
        public class InputModel
        {
            [Required]
            public string FullName { get; set; }
            [Required]
            [DataType(DataType.PhoneNumber)]
            public string PhoneNumber {get;set;}
            [Required]
            public int Age {get;set;}
        }

        [BindProperty]
        public InputModel Input {get;set;}

        // 3- Define other Bind Props and View Data and Temp Data
        [ViewData]
        public string Title {get;set;}

        // 4- define helper methods and properties
        public string ModifiedBy {get;set;}
        public string Search {get;set;}
        public void Write() {
            Console.WriteLine(ModifiedBy);
        }

        // 5- Define Page Handlers
        public async Task<IActionResult> OnGetAsync(string search) {
            var authResult=await _auth.AuthorizeAsync(User,null,
                new IAuthorizationRequirement[] {
                    new MinimumAgeRequirement(22),
                    new ForbidenCountryRequirement(new string[]{"North Korea"})});
            if (!authResult.Succeeded) {
                return Forbid();
            }
            Search=search;
            return Page();
        }

        public IActionResult OnPost() {
            ModifiedBy=DateTime.Now.ToShortDateString();
            Write();
            return Redirect("/");
        }
    }
}