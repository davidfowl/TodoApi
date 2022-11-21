using System.Security.Claims;

namespace TodoApi;

// This is a struct that we're using to bind the relevant user
// claims in a more friendly way.
public record struct UserId(string Id, bool IsAdmin)
{
    public static ValueTask<UserId> BindAsync(HttpContext context)
    {
        // Grab the id claim and check if the user is an admin
        var id = context.User.Identity!.Name!;
        var isAdmin = context.User.IsInRole("admin");

        return ValueTask.FromResult<UserId>(new(id, isAdmin));
    }
}
