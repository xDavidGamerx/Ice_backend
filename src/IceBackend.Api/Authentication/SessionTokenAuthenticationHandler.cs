using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using IceBackend.Application.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IceBackend.Api.Authentication
{
    public class SessionTokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly ISessionCache _sessionCache;

        public SessionTokenAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISessionCache sessionCache)
            : base(options, logger, encoder)
        {
            _sessionCache = sessionCache;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                return AuthenticateResult.NoResult();
            }

            var authHeaderStr = authHeader.ToString();
            if (string.IsNullOrEmpty(authHeaderStr) || !authHeaderStr.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return AuthenticateResult.Fail("Invalid Authorization Header structure. Must be 'Bearer <token>'.");
            }

            var token = authHeaderStr.Substring("Bearer ".Length).Trim();
            if (string.IsNullOrEmpty(token))
            {
                return AuthenticateResult.Fail("Session token is empty.");
            }

            try
            {
                var playerIdStr = await _sessionCache.GetPlayerIdBySessionAsync(token);
                if (string.IsNullOrEmpty(playerIdStr) || !Guid.TryParse(playerIdStr, out var playerId))
                {
                    return AuthenticateResult.Fail("Session invalid or expired.");
                }

                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, playerId.ToString()),
                    new Claim("SessionToken", token)
                };

                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return AuthenticateResult.Success(ticket);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred during session token authentication.");
                return AuthenticateResult.Fail("Authentication error.");
            }
        }
    }
}
