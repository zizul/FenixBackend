using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Presentation.Common;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace IntegrationTests.Common
{
    internal class TestAuthHandler : AuthenticationHandler<TestAuthHandlerOptions>
    {
        public const string UserIdClaim = "UserId";
        public const string RoleClaim = "Role";

        public const string SchemeName = "Test";

        public const string DefaultUserId = "0-test-user-identity-id";
        private readonly string defaultRole = Roles.USER;


        public TestAuthHandler(
            IOptionsMonitor<TestAuthHandlerOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new List<Claim>() { new Claim(ClaimTypes.Name, "test user") };

            TryAddNameIdentifierClaim(claims);
            TryAddRoleClaim(claims);

            var identity = new ClaimsIdentity(claims, SchemeName);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, SchemeName);

            var result = AuthenticateResult.Success(ticket);
            return Task.FromResult(result);
        }

        private void TryAddNameIdentifierClaim(List<Claim> claims)
        {
            // Extract User ID from the request headers if it exists,
            // otherwise use the default User ID from the options.
            if (Context.Request.Headers.TryGetValue(UserIdClaim, out var userId))
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId[0]));
            }
            else
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, DefaultUserId));
            }
        }

        private void TryAddRoleClaim(List<Claim> claims)
        {
            if (Context.Request.Headers.TryGetValue(RoleClaim, out var role))
            {
                claims.Add(new Claim(ClaimTypes.Role, role[0]));
            }
            else
            {
                claims.Add(new Claim(ClaimTypes.Role, defaultRole));
            }
        }
    }
}