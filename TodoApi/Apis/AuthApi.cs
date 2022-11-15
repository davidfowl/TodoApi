using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using TodoApi.Tests;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace TodoApi.Apis;

public static class AuthApi
{
    public static IEndpointConventionBuilder MapAuth(this IEndpointRouteBuilder routes)
    {
        // Instead of a user name and password, the user presents app with an id token
        // from an OIDC provider (we only support google). Then we exchange that for a new JWT
        // token that can be used to access the token API
        return routes.MapGet("/login/{authType}", async (string authType, HttpContext context, IConfiguration configuration) =>
        {
            var result = await context.AuthenticateAsync(authType);

            if (!result.Succeeded)
            {
                return Results.StatusCode(Status401Unauthorized);
            }

            // TODO: Clean this up
            var id = result.Principal.FindFirstValue(ClaimTypes.NameIdentifier)!;

            // REVIEW: Should we keep all of the claims?
            var claims = result.Principal.Claims.Concat(new[] { new Claim("id", id) }).ToArray();

            var token = JwtIssuer.CreateToken(configuration, claims);

            // Issue a new token
            return Results.Content(token);
        })
        .WithTags("Auth")
        .AddOpenApiSecurityRequirement();
    }

    public static TBuilder RequireJwt<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder
    {
        return builder.RequireAuthorization(pb => 
                      pb.RequireClaim("id")
                        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme));
    }
}
