using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Todo.Web.Server.Pages;

public class IndexModel : PageModel
{
    private readonly IAuthenticationSchemeProvider _schemeProvider;

    public IndexModel(IAuthenticationSchemeProvider schemeProvider)
    {
        _schemeProvider = schemeProvider;
    }

    public string[] SocialProviders { get; set; } = default!;

    public async Task OnGet()
    {
        List<string>? providers = null;

        var schemes = await _schemeProvider.GetAllSchemesAsync();
        foreach (var s in schemes)
        {
            // We're assuming all schemes that aren't cookies are social
            if (s.Name == CookieAuthenticationDefaults.AuthenticationScheme)
            {
                continue;
            }

            providers ??= new();
            providers.Add(s.Name);
        }

        SocialProviders = providers?.ToArray() ?? Array.Empty<string>();
    }
}