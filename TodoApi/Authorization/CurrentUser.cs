using System.Security.Claims;

namespace TodoApi;

// A scoped service that exposes the current user information
public class CurrentUser
{
    public TodoUser? User { get; set; }
    public ClaimsPrincipal Principal { get; set; } = default!;

    public string Id => Principal.Identity!.Name!;
    public bool IsAdmin => Principal.IsInRole("admin");
}