using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Reex.Models.v1.Wallet;
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

            try
            {
                var rehiveTokenResult = await rehiveService.VerifyUser(authHeader.Parameter);

                if (rehiveTokenResult.Status.Equals("success", StringComparison.InvariantCultureIgnoreCase))
                {
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, rehiveTokenResult.Data.Email),
                        new Claim(ClaimTypes.Name, rehiveTokenResult.Data.Email),
                        new Claim("ID", rehiveTokenResult.Data.ID.ToString())
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
            catch(Exception ex)
            {
                return AuthenticateResult.Fail("An error occurred during verification");
            }
        }
        #endregion
    }
}
