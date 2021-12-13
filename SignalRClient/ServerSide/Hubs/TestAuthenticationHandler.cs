using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

#nullable disable
namespace ServerSide.Hubs
{
    public class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }


        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new Claim[] { new Claim(ClaimTypes.Name, "johnsnow@yahoo.com"), new Claim(ClaimTypes.Role, "Admin"), new Claim(ClaimTypes.NameIdentifier, "1") };
            var claimsIdentity = new ClaimsIdentity(claims, Scheme.Name);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var ticket = new AuthenticationTicket(claimsPrincipal, Scheme.Name);

            // We can set HttpContext.User to use it in next steps
            Context.User = claimsPrincipal;
            // Context.AuthenticateAsync();// This makes infinite loop inside Authenticate handler

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

}