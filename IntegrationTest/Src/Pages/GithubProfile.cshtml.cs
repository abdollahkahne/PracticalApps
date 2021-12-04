using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Src.Services;

namespace Src.Pages
{

    public class GithubProfileModel : PageModel
    {
        public IGithubClient Client { get; set; }

        public GithubProfileModel(IGithubClient client)
        {
            Client = client;
        }

        public class InputModel
        {
            [Required]
            public string UserName { get; set; }
        }
        [BindProperty]
        public InputModel Input { get; set; }
        public GithubUser GithubUser { get; private set; }

        public async Task<IActionResult> OnGetAsync(string username)
        {
            if (username != null)
            {
                GithubUser = await Client.GetUserAsync(username);
            }
            return Page();
        }

        public IActionResult OnPost()
        {
            if (string.IsNullOrEmpty(Input.UserName))
            {
                return Page();
            }
            return RedirectToPage(new { Username = Input.UserName });
        }
    }
}