using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace Todo.Web.Server;

public static class AuthenticationExtensions
{
    public static WebApplicationBuilder AddAuthentication(this WebApplicationBuilder builder)
    {
        // Our default scheme is cookies
        var authenticationBuilder = builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme);

        // Add the default authentication cookie that will be used between the front end and
        // the backend.
        authenticationBuilder.AddCookie(o =>
        {
            o.Cookie.SameSite = SameSiteMode.Strict;
        });

        // Add social auth providers based on configuration
        //{
        //    "Authentication": {
        //        "Schemes": {
        //            "<scheme>": {
        //                "ClientId": "xxx",
        //                "ClientSecret": "xxxx"
        //            }
        //        }
        //    }
        //}

        // These are the list of social providers available to the application.
        // Many more are available from https://github.com/aspnet-contrib/AspNet.Security.OAuth.Providers
        var socialProviders = new Dictionary<string, Action<AuthenticationBuilder, Action<OAuthOptions>>>
        {
            ["GitHub"] = static (builder, configure) => builder.AddGitHub(configure),
            ["Google"] = static (builder, configure) => builder.AddGoogle(configure),
        };

        foreach (var (providerName, providerCallback) in socialProviders)
        {
            var section = builder.Configuration.GetSection($"Authentication:Schemes:{providerName}");
            if (section.Exists())
            {
                providerCallback(authenticationBuilder, options =>
                {
                    options.ClientId = section["ClientId"]!;
                    options.ClientSecret = section["ClientSecret"]!;
                    options.Events = new()
                    {
                        OnTicketReceived = async context =>
                        {
                            if (context.Principal is null)
                            {
                                context.Fail("Unable to resolve user information");
                                return;
                            }

                            // REVIEW: We could use a temporary cookie and have the user fill in their
                            // name after coming back from the redirect. Right now, we're using their email
                            // as the user name and falling back to the name.

                            // This also makes it hard to see user creation errors...

                            var id = context.Principal.FindFirstValue(ClaimTypes.NameIdentifier)!;
                            var name = (context.Principal.FindFirstValue(ClaimTypes.Email) ?? context.Principal.FindFirstValue(ClaimTypes.Name))!;

                            var client = context.HttpContext.RequestServices.GetRequiredService<TodoClient>();

                            var token = await client.GetOrCreateUserAsync(providerName, new() { Username = name, ProviderKey = id });

                            if (token is not null)
                            {
                                var result = AuthenticationHelpers.SignIn(id, name, token);

                                // Execute the result so we write the cookie to the response headers
                                await result.ExecuteAsync(context.HttpContext);
                            }

                            context.HandleResponse();

                            context.Response.Redirect("/");
                        }
                    };
                });
            }
        }

        return builder;
    }
}
