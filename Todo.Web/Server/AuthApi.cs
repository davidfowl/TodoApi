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

            if (token is null)
            {
                return Results.Unauthorized();
            }

            return SignIn(userInfo, token);
        });

        group.MapPost("login", async (UserInfo userInfo, TodoClient client) =>
        {
            // Retrieve the access token give the user info
            var token = await client.GetTokenAsync(userInfo);

            if (token is null)
            {
                return Results.Unauthorized();
            }

            return SignIn(userInfo, token);
        });

        // Social login
        group.MapGet("login/{provider}", (string provider) =>
        {
            // Trigger the social login flow
            return Results.Challenge(authenticationSchemes: new[] { provider });
        });

        group.MapPost("logout", () =>
        {
            return Results.SignOut(authenticationSchemes: new[] { CookieAuthenticationDefaults.AuthenticationScheme });
        })
        .RequireAuthorization();

        return group;
    }

    private static IResult SignIn(UserInfo userInfo, string token)
    {
        return AuthenticationHelpers.SignIn(userInfo.Username, userInfo.Username, token);
    }
}
