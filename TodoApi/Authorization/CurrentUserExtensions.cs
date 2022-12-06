using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace TodoApi;

// State that represents the current user from the database *and* the request
[Service(ServiceLifetime.Scoped)]
internal sealed class ClaimsTransformation : IClaimsTransformation
{
    private readonly CurrentUser _currentUser;
    private readonly UserManager<TodoUser> _userManager;

    public ClaimsTransformation(CurrentUser currentUser, UserManager<TodoUser> userManager)
    {
        _currentUser = currentUser;
        _userManager = userManager;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        // We're not going to transform anything. We're using this as a hook into authorization
        // to set the current user without adding custom middleware.
        _currentUser.Principal = principal;

        if (principal.FindFirstValue(ClaimTypes.NameIdentifier) is { Length: > 0 } name)
        {
            // Resolve the user manager and see if the current user is a valid user in the database
            // we do this once and store it on the current user.
            _currentUser.User = await _userManager.FindByNameAsync(name);
        }

        return principal;
    }
}