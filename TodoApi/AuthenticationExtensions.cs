using Microsoft.IdentityModel.Tokens;

namespace TodoApi;

public static class AuthenticationExtensions
{
    public static WebApplicationBuilder AddAuthentication(this WebApplicationBuilder builder)
    {
        // Configure JWT for our Todo API and one to validate google ID tokens
        var clientId = builder.Configuration["Google:ClientId"];

        builder.Services.AddAuthentication()
                        .AddJwtBearer()
                        .AddJwtBearer("Google", o =>
                        {
                            o.Authority = "https://accounts.google.com";
                            o.Audience = clientId;
                            o.TokenValidationParameters = new TokenValidationParameters()
                            {
                                ValidateAudience = true,
                                ValidIssuer = "accounts.google.com"
                            };
                            o.RequireHttpsMetadata = false;
                        });

        return builder;
    }
}
