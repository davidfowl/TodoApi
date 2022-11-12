using System.Security.Claims;

namespace TodoApi;

public record struct UserId(string Id, bool IsAdmin)
{
    public static ValueTask<UserId> BindAsync(HttpContext context)
    {
        // Grab the id claim
        var id = context.User.FindFirstValue("id")!;
        var isAdmin = context.User.IsInRole("admin");

        return ValueTask.FromResult<UserId>(new(id, isAdmin));
    }
}
