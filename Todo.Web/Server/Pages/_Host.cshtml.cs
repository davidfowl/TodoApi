using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Todo.Web.Server.Pages;

public class IndexModel : PageModel
{
    private readonly SocialProviders _socialProviders;

    public IndexModel(SocialProviders socialProviders)
    {
        _socialProviders = socialProviders;
    }

    public string[] ProviderNames { get; set; } = default!;
    public string? CurrentUserName { get; set; }

    public async Task OnGet()
    {
        ProviderNames = await _socialProviders.GetProviderNamesAsync();
        CurrentUserName = User.Identity!.Name;
    }
}