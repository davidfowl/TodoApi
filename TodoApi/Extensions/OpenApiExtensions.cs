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
            Name = "Bearer",
            Scheme = "Bearer",
            Reference = new()
            {
                Type = ReferenceType.SecurityScheme,
                Id =  "Bearer",
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

    public static IEndpointConventionBuilder Produces(this IEndpointConventionBuilder builder, string schemaReference)
    {
        return builder.WithOpenApi(o =>
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

             o.Responses.Add("200", response);
             return new(o);
         });
    }
}
