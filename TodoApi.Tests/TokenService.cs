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
        return services.AddScoped<ITokenService, TokenService>();
    }
}

public interface ITokenService
{
    // Generate a token for the specified user name and admin role
    Task<string> GenerateTokenAsync(string username, bool isAdmin = false);
}

public sealed class TokenService : ITokenService
{
    private readonly IUserClaimsPrincipalFactory<TodoUser> _claimsPrincipalFactory;
    private readonly BearerTokenOptions _options;

    public TokenService(IUserClaimsPrincipalFactory<TodoUser> claimsPrincipalFactory, 
                        IOptionsMonitor<BearerTokenOptions> options)
    {
        _claimsPrincipalFactory = claimsPrincipalFactory;

        // We're reading the authentication configuration for the Bearer scheme
        _options = options.Get(IdentityConstants.BearerScheme);
    }

    public async Task<string> GenerateTokenAsync(string username, bool isAdmin = false)
    {
        var claimsPrincipal = await _claimsPrincipalFactory.CreateAsync(new TodoUser { Id = username, UserName = username });

        if (isAdmin)
        {
            ((ClaimsIdentity?)claimsPrincipal.Identity)?.AddClaim(new(ClaimTypes.Role, "admin"));
        }

        var utcNow = (_options.TimeProvider ?? TimeProvider.System).GetUtcNow();

        var properties = new AuthenticationProperties
        {
            ExpiresUtc = utcNow + _options.BearerTokenExpiration
        };

        var ticket = CreateBearerTicket(claimsPrincipal, properties);

        static AuthenticationTicket CreateBearerTicket(ClaimsPrincipal user, AuthenticationProperties properties)
                => new(user, properties, $"{IdentityConstants.BearerScheme}:AccessToken");

        return _options.BearerTokenProtector.Protect(ticket);
    }
}
