using System.Security.Claims;

namespace TodoApi;

public record struct UserId(string Id)
{
    public static ValueTask<UserId> BindAsync(HttpContext context)
    {
        // Grab the id claim
        return ValueTask.FromResult<UserId>(new(context.User.FindFirstValue("id")!));
    }
}
