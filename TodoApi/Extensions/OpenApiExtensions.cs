using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TodoApi;

public static class OpenApiExtensions
{
    private static readonly OpenApiSecurityScheme BearerScheme = new()
    {
        Type = SecuritySchemeType.Http,
        Name = "Bearer",
        Scheme = "Bearer",
        Reference = new()
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer",
        }
    };

    public static void AddOpenApiSecurity(
            this SwaggerGenOptions swaggerGenOptions)
    {
        swaggerGenOptions.AddSecurityDefinition("Bearer", BearerScheme);
    }

    // Adds the security scheme to the Open API description
    public static IEndpointConventionBuilder AddOpenApiSecurity(this IEndpointConventionBuilder builder)
    {
        return builder.WithOpenApi(operation => new(operation)
        {
            Security =
            {
                new()
                {
                    [BearerScheme] = []
                }
            }
        });
    }
}
