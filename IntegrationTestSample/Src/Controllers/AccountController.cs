using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Src.Data.IdentityModels;

namespace Src.Controllers
{
    public class AccountController : Controller
    {
        // The UserManager is used to manage Users in Identity
        // while the SignInManager is used to perform the authentication of the users.
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult GoogleLogin()
        {
            var redirectUrl = Url.RouteUrl("Google"); // the callback path (and Not Redirect Url) is needed by provider to compare with it list of allowes redirects and then if check with them redirect the user but this is not the callback,
            // call back set in AddGoogle Service method using options.CallbackPath  which is by default is /signin-google. this path handle by google middleware and and it set the external coockie as a result.
            // And then redirect it to RedirectUrl Which passed here!! this can be each url you want for example I choose yahoo-signin!!

            var externalAuthProperties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
            return new ChallengeResult("Google", externalAuthProperties); // Here if not specified the Google it uses default authentication schema
            // return Challenge("Google");
        }

        [AllowAnonymous]
        [Route("sigin-yahoo", Name = "Google")]
        public async Task<IActionResult> GoogleResponse()
        {

            // There is at least 4 action related to authentication (linking cookie to ClaimsPrincipal which saved at HttpContext.User)
            // 1- Challange: for example in Cookie based Authentication redirect user to Login Path. If user already signed-In, calling the challange result to redirecting him to AccessDenied Path
            // 2- SigIn: Create Cookie for example to save the user authentication so it needs PrincipalClaim (claimsPrincipal =>Cookie)
            // 3- SignOut: Clear Cookie for example so it need at most schema name which can be cookie or google or ..
            // 4- Authenticate: Transform cooki information to ClaimPrincipal. Only default auth schema do this automaticly. Other wise we should do it somewhere!
            // *Login?

            // In case of using Identity with Google we have:
            // 1- Challange: which redirect user to google login page
            // 2- 


            // This returns from last external Challange Result? Does not need session? or cookie handle it? The State Parameter of returned url specify that the code belong to what request 
            var info = await _signInManager.GetExternalLoginInfoAsync(); //This is already done by middleware related to google authentication: it first request to google to verify the code it received and then sign in principal info in SignInSchema Determined for it which is by default external cookie 
            // Info has information regarding to Principal (Here user email and given name and surname) and Cookies (in exchange of code parameter in url)
            // To determine Picture url we can use Claim Transformation Mapping 
            if (info == null) // this is as a result of unauthenticated user of time out in connection to google in middleware
            {
                return RedirectToAction("Login", "Authenticate");
            }

            // As a result of Google Authentication Middleware we have a principal and a cookie. The cookie is external cookie which is different than application cookie which is single source for setting user 
            // so we should set the application cookie equal to principal which is done by signin operation and also clearing the external cookie for by passing the google authentication middleware .
            // This two thing done in External Login Sign In 

            // await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);//??? This can be done using external cookie to create User in HttpContext.User from cookie.

            // This is successfull if user already existed in Identity db (which means it is not first time login!)
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);
            if (result.Succeeded)
            {
                // Console.WriteLine(await HttpContext.GetTokenAsync("Google", "TicketCreated"));
                // Console.WriteLine(await HttpContext.GetTokenAsync("Google", "access_token"));
                // Console.WriteLine(User.Identity.IsAuthenticated); // return false since AuthenticationSchema is DefaultCookie
                return View(info.Principal);
            }
            else
            {
                // Create a user in Identity? Not necessarily??
                var user = new AppUser
                {
                    UserName = info.Principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
                    Email = info.Principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
                    Salary = "100$",
                };
                var creationResult = await _userManager.CreateAsync(user);
                if (creationResult.Succeeded)
                {
                    // Add claims to User
                    await _userManager.AddClaimsAsync(user, info.Principal.Claims);

                    // Do Login for the first time

                    var loginResult = await _userManager.AddLoginAsync(user, info); // This login user for the first time. (a new record is created in the AspNetUserLogins)

                    if (loginResult.Succeeded)
                    {
                        // var authProp = new AuthenticationProperties { IsPersistent = true };
                        // authProp.StoreTokens(info.AuthenticationTokens);
                        // await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, info.Principal, info.AuthenticationProperties); // create the application cookie but does not delete the external cookie so it is not enough!
                        var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);// This only work in case user existed and create the cookie for related user
                        await _signInManager.CreateUserPrincipalAsync(user);
                        return View(info.Principal);
                    }
                    else
                    {
                        Console.WriteLine("Something Wrong happened");
                        return View(info.Principal);
                    }
                }
                return RedirectToAction("Login", "Authenticate");
            }

        }
    }
}