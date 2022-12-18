using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Todo.Web.Server;

public static class AuthApi
{
    private static readonly string ExternalProviderKey = "ExternalProviderName";

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

        group.MapPost("logout", async (HttpContext context) =>
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // TODO: Support remote logout
            // var result = await context.AuthenticateAsync();
            // If this is an external login then use it
            //if (result.Properties?.Items.TryGetValue(ExternalProviderKey, out var externalProviderName) == true)
            //{
            //    await context.SignOutAsync(externalProviderName, new() { RedirectUri = "/" });
            //}
        })
        .RequireAuthorization();

        // Social login
        group.MapGet("login/{provider}", (string provider) =>
        {
            // Trigger the external login flow by issuing a challenge with the provider name.
            // This name maps to the registered authentication scheme names in AuthenticationExtensions.cs
            return Results.Challenge(
                properties: new() { RedirectUri = $"/auth/signin/{provider}" },
                authenticationSchemes: new[] { provider });
        });

        group.MapGet("signin/{provider}", async (string provider, TodoClient client, HttpContext context) =>
        {
            // Grab the login information from the external login dance
            var result = await context.AuthenticateAsync(AuthConstants.ExternalScheme);

            if (result.Succeeded)
            {
                var principal = result.Principal;

                var id = principal.FindFirstValue(ClaimTypes.NameIdentifier)!;

                // TODO: We should have the user pick a user name to complete the external login dance
                // for now we'll prefer the email address
                var name = (principal.FindFirstValue(ClaimTypes.Email) ?? principal.Identity?.Name)!;

                var token = await client.GetOrCreateUserAsync(provider, new() { Username = name, ProviderKey = id });

                if (token is not null)
                {
                    // Write the login cookie
                    await SignIn(id, name, token, provider).ExecuteAsync(context);
                }
            }

            // Delete the external cookie
            await context.SignOutAsync(AuthConstants.ExternalScheme);

            // TODO: Handle the failure somehow

            return Results.Redirect("/");
        });

        return group;
    }

    private static IResult SignIn(UserInfo userInfo, string token)
    {
        return SignIn(userInfo.Username, userInfo.Username, token, providerName: null);
    }

    private static IResult SignIn(string userId, string userName, string token, string? providerName)
    {
        var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId));
        identity.AddClaim(new Claim(ClaimTypes.Name, userName));

        var properties = new AuthenticationProperties();

        // Store the external provider name so we can do remote sign out
        if (providerName is not null)
        {
            properties.Items[ExternalProviderKey] = providerName;
        }

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
