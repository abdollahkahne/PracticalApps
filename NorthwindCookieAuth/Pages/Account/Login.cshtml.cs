using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NorthwindCookieAuth.Data;


// [AllowAnonymous] bypasses all authorization statements
// For example if you apply [AllowAnonymous] at the controller level, any [Authorize] attributes on any action within it is ignored.
[AllowAnonymous]
public class LoginModel:PageModel {
    // define class for input model here inside the class
    public class InputModel {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

    //define constructor to inject services/dependencies and their private readonly fields / getter props
    public LoginModel()
    {
        
    }

    // Define Bind Properties, Temp Data and View/Bag Data (they should be property not field)
    [BindProperty]
    public InputModel Input {get;set;}
    [TempData]
    public string ErrorMessage { get; set; }

    // Define Properties which used for data saving but not binded
    public string ReturnUrl { get; set; }

    // Define Private/public helper methods
    private async Task<ApplicationUser> AuthenticateUserAsync(string username,string password) {
        // Do busines logic for authentication here and return the User at the end
        await Task.Delay(500);
        // if (true) {
        //     return null;
        // }
        return new ApplicationUser {
            Email="john@contoso.com",
            FullName="John Snow",
            Modified=DateTime.Now,
            Country="USA",
            Age=25,
        };
    }

    // Defin Page Handlers (OnGet,....)
    public async Task<IActionResult> OnGetAsync(string returnUrl) {
        if (!string.IsNullOrEmpty(ErrorMessage)) {
            ModelState.AddModelError(string.Empty,ErrorMessage);
        }

        // Clear existing Cookies
        #region Snippet2
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        #endregion
        ReturnUrl=returnUrl;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync() {
        ReturnUrl??="/";
        // Check Validity of Model
        if (!ModelState.IsValid) {
            return Page();
        }
        var user=await AuthenticateUserAsync(Input.Email,Input.Password);

        // Check case of wrong user-pass
        if (user==null) {
            ModelState.AddModelError(string.Empty,"Invalid Username and Password");
            return Page();
        }

        #region snippet1
            // define claims as a list of claim (Name and Role is mandatory)
            var claims=new List<Claim> {
                new Claim(ClaimTypes.Name,user.Email),
                new Claim("FullName",user.FullName),
                new Claim(ClaimTypes.Role,"Admin"),
                new Claim("Modified",user.Modified.ToString()),
                new Claim("Country",user.Country),
                new Claim("Age",user.Age.ToString()),
            };

            // create claim Identity using claims and Authentication Schema
            // claim Identity has IsAuthenticated and AuthenticationType which is related to Schema
            var claimIdentity=new ClaimsIdentity(claims,CookieAuthenticationDefaults.AuthenticationScheme);

            // create claim principal (this is saved at HttpContext.User in UseAuthentication Middleware)
            // It made from one or multiple claim Identities
            // It has some helper methods like IsInRole or HasClaim
            var claimsPrincipal=new ClaimsPrincipal(claimIdentity);

            //define Auth Properties
            var authProps=new AuthenticationProperties {
                IsPersistent=false,
            };

            // use SignInAsync Method to create and set cookie
            // (and UseAuthentication Middleware use this to generate User props)
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                claimsPrincipal,
                authProps
            );
            return Redirect(ReturnUrl);

        #endregion
    }

}