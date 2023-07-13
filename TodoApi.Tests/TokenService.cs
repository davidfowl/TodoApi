using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.Extensions.Options;

namespace TodoApi;

public static class AuthenticationServiceExtensions
{
    public static IServiceCollection AddTokenService(this IServiceCollection services)
    {
        // Wire up the token service
        return services.AddSingleton<ITokenService, TokenService>();
    }
}

public interface ITokenService
{
    // Generate a token for the specified user name and admin role
    string GenerateToken(string username, bool isAdmin = false);
}

public sealed class TokenService : ITokenService
{
    private BearerTokenOptions _options;

    public TokenService(IOptionsMonitor<BearerTokenOptions> options)
    {
        // We're reading the authentication configuration for the Bearer scheme
        _options = options.Get(BearerTokenDefaults.AuthenticationScheme);
    }

    public string GenerateToken(string username, bool isAdmin = false)
    {
        var identity = new ClaimsIdentity(BearerTokenDefaults.AuthenticationScheme);

        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, username));

        if (isAdmin)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, "admin"));
        }

        var utcNow = (_options.TimeProvider ?? TimeProvider.System).GetUtcNow();

        var properties = new AuthenticationProperties
        {
            ExpiresUtc = utcNow + _options.BearerTokenExpiration
        };

        var ticket = CreateBearerTicket(new ClaimsPrincipal(identity), properties);

        static AuthenticationTicket CreateBearerTicket(ClaimsPrincipal user, AuthenticationProperties properties)
                => new(user, properties, $"{BearerTokenDefaults.AuthenticationScheme}:AccessToken");

        return _options.BearerTokenProtector.Protect(ticket);
    }
}
