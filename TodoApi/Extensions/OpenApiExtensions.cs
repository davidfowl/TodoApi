using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.OpenApi.Models;

namespace TodoApi;

public static class OpenApiExtensions
{
    // Adds the security scheme to the Open API description
    public static IEndpointConventionBuilder AddOpenApiSecurityRequirement(this IEndpointConventionBuilder builder)
    {
        const string schemeName = "Bearer";

        var scheme = new OpenApiSecurityScheme()
        {
            Type = SecuritySchemeType.Http,
            Name = schemeName,
            Scheme = schemeName,
            Reference = new()
            {
                Type = ReferenceType.SecurityScheme,
                Id = schemeName
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
