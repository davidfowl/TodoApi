using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace Todo.Web.Server;

public class AuthConstants
{
    public static string SocialScheme { get; } = "Social";
}

public static class AuthenticationExtensions
{
    private delegate void OAuthProvider(AuthenticationBuilder authenticationBuilder, Action<OAuthOptions> configure);

    public static WebApplicationBuilder AddAuthentication(this WebApplicationBuilder builder)
    {
        // Our default scheme is cookies
        var authenticationBuilder = builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme);

        // Add the default authentication cookie that will be used between the front end and
        // the backend.
        authenticationBuilder.AddCookie();

        // This is the cookie that will store the user information from the social login provider
        authenticationBuilder.AddCookie(AuthConstants.SocialScheme);

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
        var socialProviders = new Dictionary<string, OAuthProvider>
        {
            ["GitHub"] = static (builder, configure) => builder.AddGitHub(configure),
            ["Google"] = static (builder, configure) => builder.AddGoogle(configure),
            ["Microsoft"] = static (builder, configure) => builder.AddMicrosoftAccount(configure),
        };

        foreach (var (providerName, provider) in socialProviders)
        {
            var section = builder.Configuration.GetSection($"Authentication:Schemes:{providerName}");
            if (section.Exists())
            {
                provider(authenticationBuilder, options =>
                {
                    options.ClientId = section[nameof(options.ClientId)]!;
                    options.ClientSecret = section[nameof(options.ClientSecret)]!;

                    // This will save the information in the cookie
                    options.SignInScheme = AuthConstants.SocialScheme;
                });
            }
        }

        // Add the service that resolves social providers so we can show them in the UI
        builder.Services.AddSingleton<SocialProviders>();

        return builder;
    }
}
