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
                    [BearerScheme] = new List<string>()
                }
            }
        });
    }

    public static IEndpointConventionBuilder Produces(this IEndpointConventionBuilder builder, string schemaReference)
    {
        return builder.WithOpenApi(operation =>
         {
             var response = new OpenApiResponse
             {
                 Description = "Success",
                 Content =
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = schemaReference }
                        }
                    }
                }
             };

             operation.Responses.Add("200", response);
             return new(operation);
         });
    }
}
