using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace Todo.Web.Server;

public class AuthConstants
{
    public static string ExternalScheme { get; } = "External";
}

public static class AuthenticationExtensions
{
    private delegate void ExternalAuthProvider(AuthenticationBuilder authenticationBuilder, Action<object> configure);

    public static WebApplicationBuilder AddAuthentication(this WebApplicationBuilder builder)
    {
        // Our default scheme is cookies
        var authenticationBuilder = builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme);

        // Add the default authentication cookie that will be used between the front end and
        // the backend.
        authenticationBuilder.AddCookie();

        // This is the cookie that will store the user information from the external login provider
        authenticationBuilder.AddCookie(AuthConstants.ExternalScheme);

        // Add external auth providers based on configuration
        //{
        //    "Authentication": {
        //        "Schemes": {
        //            "<scheme>": {
        //                "ClientId": "xxx",
        //                "ClientSecret": "xxxx"
        //                etc..
        //            }
        //        }
        //    }
        //}

        // These are the list of external providers available to the application.
        // Many more are available from https://github.com/aspnet-contrib/AspNet.Security.OAuth.Providers
        var externalProviders = new Dictionary<string, ExternalAuthProvider>
        {
            ["GitHub"] = static (builder, configure) => builder.AddGitHub(configure),
            ["Google"] = static (builder, configure) => builder.AddGoogle(configure),
            ["Microsoft"] = static (builder, configure) => builder.AddMicrosoftAccount(configure),
            ["Auth0"] = static (builder, configure) => builder.AddAuth0WebAppAuthentication(configure)
                                                              .WithAccessToken(configure),
        };

        foreach (var (providerName, provider) in externalProviders)
        {
            var section = builder.Configuration.GetSection($"Authentication:Schemes:{providerName}");

            if (section.Exists())
            {
                provider(authenticationBuilder, options =>
                {
                    // Bind this section to the specified options
                    section.Bind(options);

                    // This will save the information in the external cookie
                    if (options is RemoteAuthenticationOptions remoteAuthenticationOptions)
                    {
                        remoteAuthenticationOptions.SignInScheme = AuthConstants.ExternalScheme;
                    }
                    else if (options is Auth0WebAppOptions auth0WebAppOptions)
                    {
                        // Skip the cookie handler since we already add it
                        auth0WebAppOptions.SkipCookieMiddleware = true;
                    }
                });

                if (providerName is "Auth0")
                {
                    // Set this up once
                    SetAuth0SignInScheme(builder);
                }
            }
        }

        // Add the service that resolves external providers so we can show them in the UI
        builder.Services.AddSingleton<ExternalProviders>();

        return builder;

        static void SetAuth0SignInScheme(WebApplicationBuilder builder)
        {
            builder.Services.AddOptions<OpenIdConnectOptions>(Auth0Constants.AuthenticationScheme)
                .PostConfigure(o =>
                {
                    // The Auth0 APIs don't let you set the sign in scheme, it defaults to the default sign in scheme.
                    // Use named options to configure the underlying OpenIdConnectOptions's sign in scheme instead.
                    o.SignInScheme = AuthConstants.ExternalScheme;
                });
        }
    }

    private static readonly string ExternalProviderKey = "ExternalProviderName";
    private static readonly string HasExternalTokenKey = "ExternalToken";

    public static string? GetExternalProvider(this AuthenticationProperties properties) =>
        properties.GetString(ExternalProviderKey);

    public static void SetExternalProvider(this AuthenticationProperties properties, string providerName) =>
        properties.SetString(ExternalProviderKey, providerName);

    public static bool HasExternalToken(this AuthenticationProperties properties) =>
        properties.GetString(HasExternalTokenKey) is not null;

    public static void SetHasExternalToken(this AuthenticationProperties properties, bool hasToken)
    {
        if (hasToken)
        {
            properties.SetString(HasExternalTokenKey, "1");
        }
        else
        {
            properties.Items.Remove(HasExternalTokenKey);
        }
    }
}
