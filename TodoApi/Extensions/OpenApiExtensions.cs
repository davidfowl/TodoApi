using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.OpenApi.Models;

namespace TodoApi;

public static class OpenApiExtensions
{
    // Adds the security scheme to the Open API description
    public static IEndpointConventionBuilder AddOpenApiSecurityRequirement(this IEndpointConventionBuilder builder)
    {
        var scheme = new OpenApiSecurityScheme()
        {
            Type = SecuritySchemeType.Http,
            Name = AuthenticationHelper.BearerTokenScheme,
            Scheme = AuthenticationHelper.BearerTokenScheme,
            Reference = new()
            {
                Type = ReferenceType.SecurityScheme,
                Id = AuthenticationHelper.BearerTokenScheme
            }
        };

        return builder.WithOpenApi(operation => new(operation)
        {
            Security =
            {
                new()
                {
                    [scheme] = new List<string>()
                }
            }
        });
    }

}
