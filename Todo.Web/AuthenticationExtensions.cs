using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace Todo.Web;

public static class AuthenticationExtensions
{
    public static WebApplicationBuilder AddAuthentication(this WebApplicationBuilder builder)
    {
        // Configure cookie auth and social auth
        // We're also storing the ID token since there's no local user management
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddGoogle(o =>
            {
                o.SaveTokens = true;
                o.Events = new OAuthEvents()
                {
                    OnCreatingTicket = o =>
                    {
                        if (o.TokenResponse.Response?.RootElement.GetProperty("id_token").GetString() is string idTokenValue)
                        {
                            var tokens = o.Properties.GetTokens();
                            var idToken = new AuthenticationToken
                            {
                                Name = "id",
                                Value = idTokenValue
                            };
                            // Add the id token
                            o.Properties.StoreTokens(tokens.Concat(new[] { idToken }));
                        }
                        else
                        {
                            // Unable to find the auth token
                            o.NoResult();
                        }
                        return Task.CompletedTask;
                    },
                };
                o.CorrelationCookie = new CookieBuilder
                {
                    SecurePolicy = CookieSecurePolicy.Always,
                };

                o.ClientId = builder.Configuration["Google:ClientId"]!;
                o.ClientSecret = builder.Configuration["Google:ClientSecret"]!;
            })
            .AddCookie();

        return builder;
    }
}
