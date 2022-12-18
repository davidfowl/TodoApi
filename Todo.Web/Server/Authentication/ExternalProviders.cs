using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Todo.Web.Server;

public class ExternalProviders
{
    private readonly IAuthenticationSchemeProvider _schemeProvider;
    private Task<string[]>? _providerNames;

    public ExternalProviders(IAuthenticationSchemeProvider schemeProvider)
    {
        _schemeProvider = schemeProvider;
    }

    public Task<string[]> GetProviderNamesAsync()
    {
        return _providerNames ??= GetProviderNamesAsyncCore();
    }

    private async Task<string[]> GetProviderNamesAsyncCore()
    {
        List<string>? providerNames = null;

        var schemes = await _schemeProvider.GetAllSchemesAsync();

        foreach (var s in schemes)
        {
            // We're assuming all schemes that aren't cookies are social
            if (s.Name == CookieAuthenticationDefaults.AuthenticationScheme ||
                s.Name == AuthConstants.ExternalScheme)
            {
                continue;
            }

            providerNames ??= new();
            providerNames.Add(s.Name);
        }

        return providerNames?.ToArray() ?? Array.Empty<string>();
    }
}
