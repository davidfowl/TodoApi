using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Todo.Web.Server;

public class ExternalProviders(IAuthenticationSchemeProvider schemeProvider)
{
    private Task<string[]>? _providerNames;

    public Task<string[]> GetProviderNamesAsync()
    {
        return _providerNames ??= GetProviderNamesAsyncCore();
    }

    private async Task<string[]> GetProviderNamesAsyncCore()
    {
        List<string>? providerNames = null;

        var schemes = await schemeProvider.GetAllSchemesAsync();

        foreach (var s in schemes)
        {
            // We're assuming all schemes that aren't cookies are social
            if (s.Name == CookieAuthenticationDefaults.AuthenticationScheme ||
                s.Name == AuthenticationSchemes.ExternalScheme)
            {
                continue;
            }

            providerNames ??= [];
            providerNames.Add(s.Name);
        }

        return providerNames?.ToArray() ?? [];
    }
}
