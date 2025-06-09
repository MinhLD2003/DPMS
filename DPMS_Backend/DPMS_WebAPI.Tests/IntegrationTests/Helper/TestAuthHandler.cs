using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace DPMS_WebAPI.Tests.IntegrationTests.Helper
{
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(
           IOptionsMonitor<AuthenticationSchemeOptions> options,
           ILoggerFactory logger,
           UrlEncoder encoder,
           ISystemClock clock)
           : base(options, logger, encoder, clock)
        {
        }
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var testUserId = "E02EC95C-5C94-470A-88EA-29CC32BCA9D5";
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, testUserId),
                new Claim(ClaimTypes.Name, "testuser@example.com"),
                new Claim(ClaimTypes.Email, "testuser@example.com"),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim("Permission", "ADMIN_DPMS"),
                new Claim("Permission", "BOD"),
                new Claim("Permission", "DPO"),
                new Claim("Feature", "IssueTicketManagement"),
                new Claim("Feature", "RiskManagement")
            };

            // Create the identity using the claims and the "Test" authentication scheme
            var identity = new ClaimsIdentity(claims, "Test");

            // Create the claims principal from the identity
            var principal = new ClaimsPrincipal(identity);

            // Create the authentication ticket with the principal and scheme name
            var ticket = new AuthenticationTicket(principal, "Test");

            // Return a successful authentication result with the ticket
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
