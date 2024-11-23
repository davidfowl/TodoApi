using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.Extensions.Options;

namespace TodoApi;

public sealed class TokenService(SignInManager<TodoUser> signInManager, IOptionsMonitor<BearerTokenOptions> options)
{
    private readonly BearerTokenOptions _options = options.Get(IdentityConstants.BearerScheme);

    public async Task<string> GenerateTokenAsync(string username, bool isAdmin = false)
    {
        var claimsPrincipal = await signInManager.CreateUserPrincipalAsync(new TodoUser { Id = username, UserName = username });

        if (isAdmin)
        {
            ((ClaimsIdentity?)claimsPrincipal.Identity)?.AddClaim(new(ClaimTypes.Role, "admin"));
        }

        // This is copied from https://github.com/dotnet/aspnetcore/blob/238dabc8bf7a6d9485d420db01d7942044b218ee/src/Security/Authentication/BearerToken/src/BearerTokenHandler.cs#L66
        var timeProvider = _options.TimeProvider ?? TimeProvider.System;

        var utcNow = timeProvider.GetUtcNow();

        var properties = new AuthenticationProperties
        {
            ExpiresUtc = utcNow + _options.BearerTokenExpiration
        };

        var ticket = new AuthenticationTicket(
            claimsPrincipal, properties, $"{IdentityConstants.BearerScheme}:AccessToken");

        return _options.BearerTokenProtector.Protect(ticket);
    }
}
