using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Todo.Web.Server;

public static class AuthApi
{
    public static RouteGroupBuilder MapAuth(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/auth");

        group.MapPost("register", async (UserInfo userInfo, TodoClient client) =>
        {
            // Retrieve the access token give the user info
            var token = await client.CreateUserAsync(userInfo);

            return CreateCookieFromToken(userInfo, token);
        });

        group.MapPost("login", async (UserInfo userInfo, TodoClient client) =>
        {
            // Retrieve the access token give the user info
            var token = await client.GetTokenAsync(userInfo);

            return CreateCookieFromToken(userInfo, token);
        });

        group.MapPost("logout", () =>
        {
            return Results.SignOut(authenticationSchemes: new[] { CookieAuthenticationDefaults.AuthenticationScheme });
        })
        .RequireAuthorization();

        return group;
    }

    private static IResult CreateCookieFromToken(UserInfo userInfo, string? token)
    {
        if (token is null)
        {
            return Results.Unauthorized();
        }

        var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userInfo.Username));

        var properties = new AuthenticationProperties();
        var tokens = new[]
        {
            new AuthenticationToken { Name = TokenNames.AccessToken, Value = token }
        };

        properties.StoreTokens(tokens);

        return Results.SignIn(new ClaimsPrincipal(identity),
            properties: properties,
            authenticationScheme: CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
