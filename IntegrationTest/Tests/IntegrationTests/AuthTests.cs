using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Src;
using Src.Services;
using Tests.Helpers;
using Xunit;

namespace Tests.IntegrationTests
{
    public class AuthTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly CustomWebApplicationFactory<Startup> _factory;
        private readonly HttpClient _client;

        public AuthTests(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GitHubProfilePage__GetUserInfo()
        {
            //Given
            var username = "user";
            var expected = "User"; // The login Name of Github User
            var factory = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(AddGitHubClient)); // We used here ConfigureTesetService which Surely run after Configure Service for both Generic Host and Web Host altough I think this method always run with new configuration in Generic Host but in WebHost we should use Configure Test Service
            var client = factory.CreateClient();

            //When
            var response = await client.GetAsync("/GithubProfile");
            response.EnsureSuccessStatusCode();

            var htmlDocument = await HtmlHelpers.GetDocumentAsync(response);
            var form = htmlDocument.QuerySelector("#user-profile") as IHtmlFormElement;
            var submitBtn = htmlDocument.QuerySelector("#user-profile button") as IHtmlElement;
            var formValues = new Dictionary<string, string>();
            formValues.Add("Input.UserName", username);
            var userProfile = await client.SubmitFormAsync(form, submitBtn, formValues); // since it do auto redirects it has the last response which is page with user data
            userProfile.EnsureSuccessStatusCode();

            var userProfileHtml = await HtmlHelpers.GetDocumentAsync(userProfile);
            var actual = userProfileHtml.QuerySelector("#user-login").TextContent;

            //Then
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task GetSecurePage_UserIsNotAuthenticated_RedirectToLoginPage()
        {
            //Given
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            //When
            var response = await client.GetAsync("/SecurePage");
            // response.EnsureSuccessStatusCode();// This make an exception for redirect so do not use it

            //Then
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.StartsWith("http://localhost/identity/account/login", response.Headers.Location.AbsoluteUri.ToLower());
        }

        [Fact]
        public async Task GetSecurePage_UserIsAuthenticated_PageReturned()
        {
            //Given
            var factory = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Here we add a test schema and make it default
                    // To add an authentication scheme, we call AddScheme<TOptions, THandler>(string, Action<TOptions>) on the AuthenticationBuilder.
                    // TOption Derived from AuthenticationSchemeOptions which include forwarded scheme for multiple authentication scheme, events to hook multiple steps of authentication process
                    // THandler derived from  AuthenticationHandler<TOption> or IAuthenticationHandler<T>
                    // Here we can name the sheme and sets its option too:
                    // If we use abstract class instead of interface for handler only one method needs override which is: HandleAuthenticateAsync handle do the Authenticate step (which return AuthenticationResult which is fail or success with a ticket including principal)
                    // In case we want to define a remote scheme we should use RemoteAuthenticationHandler<TOptions> and RemoteAuthenticationOption instead to inherit from
                    // The authentication handler is not a singleton, so you can use any services from DI. The handler is instantiated once per request. so we can inject every type dependency to handler
                    services.AddAuthentication(TestAuthenticationSchema).AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(TestAuthenticationSchema, options =>
                    {
                        // options.ForwardAuthenticate="Cookie";
                        // options.ForwardSignIn="Cookie";
                        // options.ForwardSignOut="Cookie";
                        options.ClaimsIssuer = "Test Issuer";
                    });
                });
            });
            var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            // var request = new HttpRequestMessage(HttpMethod.Get, "/securepage");
            // request.Headers.Add("Authorization", "Test");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TestAuthenticationSchema); // This is used for specifying authentication scheme used in Test!
            //When
            // var response = await client.SendAsync(request);
            var response = await client.GetAsync("/securepage");
            // response.EnsureSuccessStatusCode(); // since this prevent code from continuing in case other than 2xx comment it

            //Then
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // implement a test simplified Github Client
        private void AddGitHubClient(IServiceCollection services)
        {
            services.AddSingleton<IGithubClient>(new TestGithubClient());
        }

        public const string TestAuthenticationSchema = "Test";
    }

    // HandleChallengeAsync and HandleForbiddenAsync:
    // These two are methods you can override in your handler to influence what happens when an authentication challenge (401) or a forbidden response (403) is returned from later layers.
    // The base class implementations set the response status codes to 401 and 403 respectively.
    internal class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        // Basic Authentication Implementation
        // protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        // {
        //     // Firsrt check the Authorization Header is presented and it is valid (Authorization:Basic ....)
        //     if (!Request.Headers.ContainsKey("Authorization"))
        //     {
        //         return Task.FromResult(AuthenticateResult.NoResult()); // We try to return the NoResult to continue to other handlers. In case of Fail it return Forbid but in this case it return Challange
        //     }
        //     if (!AuthenticationHeaderValue.TryParse(Request.Headers["Authorization"], out AuthenticationHeaderValue headerValue))
        //     {
        //         return Task.FromResult(AuthenticateResult.NoResult());
        //     }
        //     // The header can be Basic or Bearer we check it here to determine the true handler for it
        //     if (!headerValue.Scheme.Equals("Basic", StringComparison.OrdinalIgnoreCase))
        //     {
        //         return Task.FromResult(AuthenticateResult.NoResult());
        //     }
        //     // decode username and password from header (They sent in base 64 string encoding and we should decode them)
        //     var bytes = Convert.FromBase64String(headerValue.Parameter);
        //     var utf8format = Encoding.UTF8.GetString(bytes);
        //     var parts = utf8format.Split(":");
        //     if (parts.Length != 2)
        //     {
        //         return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization header format!"));
        //     }
        //     string username = parts[0], password = parts[1];
        //     // Here we should check username password is OK with any service we can! and return a boolean indicating status of authentication and claim if required!!
        //     bool isUserPassValid = new Random().Next(1, 100) > 50 ? true : false;

        //     if (!isUserPassValid)
        //     {
        //         return Task.FromResult(AuthenticateResult.Fail("Invalid username or password"));
        //     }
        //     var claims = new[] { new Claim(ClaimTypes.Name, username) };
        //     var identity = new ClaimsIdentity(claims, Scheme.Name);
        //     var principal = new ClaimsPrincipal(identity);
        //     var ticket = new AuthenticationTicket(principal, Scheme.Name);
        //     return Task.FromResult(AuthenticateResult.Success(ticket));
        // }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new Claim[] { new Claim(ClaimTypes.Name, "johnsnow@yahoo.com"), new Claim(ClaimTypes.Role, "Admin") };
            var claimsIdentity = new ClaimsIdentity(claims, Scheme.Name);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var ticket = new AuthenticationTicket(claimsPrincipal, Scheme.Name);

            // We can set HttpContext.User to use it in next steps
            Context.User = claimsPrincipal;
            // Context.AuthenticateAsync();// This makes infinite loop inside Authenticate handler

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        // // if we want to implement for example Basic Authentication which receive username and password in plain text for authentication we should add following:
        // protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        // {
        //     // Add WWW-Authenticate which includes Realm value for browser
        //     //This is the standard header the server is required to return when authentication is needed. In a browser scenario, returning this header triggers the login pop-up.
        //     Response.Headers["WWW-Authenticate"] = $"Basic realm=\"{Options.Realm}\", charset=\"UTF-8\"";
        //     return base.HandleChallengeAsync(properties);
        // }

        // protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
        // {
        //     return base.HandleForbiddenAsync(properties);
        // }
    }

    internal class TestGithubClient : IGithubClient
    {
        public Task<GithubUser> GetUserAsync(string username)
        {
            if (username == "user")
            {
                var user = new GithubUser
                {
                    Login = "User",
                    Company = "Contoso Blockchain",
                    Name = "Jon Snow",
                };
                return Task.FromResult(user);
            }
            else
            {
                return Task.FromResult<GithubUser>(null);
            }
        }
    }
}




// ** Authentication Service in Asp.Net Core 
// Authentication is the process of determining a user's identity. Authorization is the process of determining whether a user has access to a resource.
// In ASP.NET Core, authentication is handled by the IAuthenticationService, which is used by authentication middleware.
// The authentication service uses registered authentication handlers to complete authentication-related actions.
// Examples of authentication-related actions include:
//     Authenticating a user.
//     Responding when an unauthenticated user tries to access a restricted resource.

// The registered authentication handlers and their configuration options are called "schemes". (We can have different schema for different actions in Auth process for example cookie for signin or signout or challange or forbid or authenticate)
// Authentication schemes are specified by registering authentication services in Startup.ConfigureServices:

// By calling a scheme-specific extension method after a call to services.AddAuthentication (such as AddJwtBearer or AddCookie, for example)
// Less commonly, by calling AuthenticationBuilder.AddScheme directly.


// approaches to select which authentication handler is responsible for generating the correct set of claims:

// Authentication scheme determined by attributes or policies!
// The default authentication scheme,specified in AddAuthentication.
// Directly set HttpContext.User (Without using authentication handler/scheme). We can set it directly or using HttpContext.AuthenticateAsync(principal);

// An authentication handler:
//     Is a type that implements the behavior of a scheme.
//     Is derived from IAuthenticationHandler or AuthenticationHandler<TOptions>.
//     Has the primary responsibility to authenticate users.

// Based on the authentication scheme's configuration and the incoming request context, authentication handlers:
//     Construct AuthenticationTicket objects representing the user's identity if authentication is successful.
//     Return 'no result' or 'failure' if authentication is unsuccessful.
//     Have methods for challenge and forbid actions for when users attempt to access resources:
//         They are unauthorized to access (forbid).
//         When they are unauthenticated (challenge).


// RemoteAuthenticationHandler<TOptions> is the class for authentication that requires a remote authentication step.
// When the remote authentication step is finished, the handler calls back to the CallbackPath set by the handler.
// The handler finishes the authentication step using the information passed to the HandleRemoteAuthenticateAsync callback path.
// OAuth 2.0 and OIDC both use this pattern.
// JWT and cookies do not since they can just directly use the bearer header and cookie to authenticate.
// The remotely hosted provider in this case:
//     Is the authentication provider.
//     Examples include Facebook, Twitter, Google, Microsoft, and any other OIDC provider which handles authenticating users using the handlers mechanism.

// Authenticat: HttpContext.AuthenticateAsync()<==>DefaultAuthenticationScheme.HandleAuthenticationAsync()
// An authentication scheme's authenticate action is responsible for constructing the user's identity based on request context.
// It returns an AuthenticateResult indicating whether authentication was successful and, if so, the user's identity in an authentication ticket. Authenticate examples include:
//     A cookie authentication scheme constructing the user's identity from cookies.
//     A JWT bearer scheme deserializing and validating a JWT bearer token to construct the user's identity.

// Challenge: HttpContext.ChallengeAsync()<==>DefaultAuthenticationScheme.HandleChallengeAsync()
// An authentication challenge is invoked by Authorization when an unauthenticated user requests an endpoint that requires authentication.
// An authentication challenge is issued, for example, when an anonymous user requests a restricted resource or clicks on a login link.
// Authorization invokes a challenge using the specified authentication scheme(s), or the default if none is specified. Authentication challenge examples include:
//     A cookie authentication scheme redirecting the user to a login page.
//     A JWT bearer scheme returning a 401 result with a www-authenticate: bearer header.

// A challenge action should let the user know what authentication mechanism to use to access the requested resource.

// Forbid: HttpContext.ForbidAsync()<==>DefaultAuthenticationScheme.HandleForbiddenAsync()
// An authentication scheme's forbid action is called by Authorization when an authenticated user attempts to access a resource they are not permitted to access. Authentication forbid examples include:
//     A cookie authentication scheme redirecting the user to a page indicating access was forbidden.
//     A JWT bearer scheme returning a 403 result.
//     A custom authentication scheme redirecting to a page where the user can request access to the resource.

// A forbid action can let the user know:
//     They are authenticated.
//     They aren't permitted to access the requested resource.




