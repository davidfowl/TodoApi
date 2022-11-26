using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace Todo.Web.Server;

public static class AuthApi
{
    public static RouteGroupBuilder MapAuth(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/auth");

        group.MapPost("login", async (UserInfo userInfo, IHttpClientFactory clientFactory) =>
        {
            var todoClient = clientFactory.CreateClient("TodoApi");
            var response = await todoClient.PostAsJsonAsync("users/token", userInfo);

            if (!response.IsSuccessStatusCode)
            {
                return Results.Unauthorized();
            }

            var token = await response.Content.ReadFromJsonAsync<AuthToken>();

            if (token is null)
            {
                return Results.Unauthorized();
            }

            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userInfo.Username));

            var properties = new AuthenticationProperties();
            properties.SetString("token", token.Token);

            return Results.SignIn(new ClaimsPrincipal(identity),
                properties: properties,
                authenticationScheme: CookieAuthenticationDefaults.AuthenticationScheme);
        });

        group.MapPost("logout", () =>
        {
            return Results.SignOut(authenticationSchemes: new[] { CookieAuthenticationDefaults.AuthenticationScheme });
        })
        .RequireAuthorization();

        return group;
    }
}
