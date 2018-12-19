using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Reex.Services.RehiveService;

namespace Reex.Helpers
{
    public class TokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        #region fields
        private readonly IRehiveService rehiveService;
        #endregion

        #region constructors
        public TokenAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IRehiveService rehiveService) : base(options, logger, encoder, clock)
        {
            this.rehiveService = rehiveService;
        }
        #endregion

        #region methods
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return AuthenticateResult.Fail("Missing Authorization Header");
            }

            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);

            if (!authHeader.Scheme.Equals("token", StringComparison.InvariantCultureIgnoreCase))
            {
                return AuthenticateResult.Fail("Invalid Authorization Scheme");
            }

            var rehiveTokenResult = await rehiveService.VerifyUser(authHeader.Parameter);

            if (rehiveTokenResult.Status.Equals("success", StringComparison.InvariantCultureIgnoreCase))
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, rehiveTokenResult.Data.Email),
                    new Claim(ClaimTypes.Name, rehiveTokenResult.Data.Email)
                };

                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return AuthenticateResult.Success(ticket);
            }
            else
            {
                return AuthenticateResult.Fail($"{rehiveTokenResult.Status} - {rehiveTokenResult.Message}");
            }
        }
        #endregion
    }
}
