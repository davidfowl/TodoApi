using Microsoft.AspNetCore.Identity;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace TodoApi;

public static class UsersApi
{
    public static RouteGroupBuilder MapUsers(this RouteGroupBuilder group)
    {
        group.WithTags("Users");

        group.MapPost("/", async (NewUser newUser, UserManager<TodoUser> userManager) =>
        {
            if (newUser.Password is null)
            {
                return Results.Problem();
            }

            var result = await userManager.CreateAsync(new() { UserName = newUser.Username }, newUser.Password);

            if (result.Succeeded)
            {
                return Results.Ok();
            }

            return Results.BadRequest();
        })
        .Produces(Status200OK)
        .ProducesProblem(Status400BadRequest)
        .Produces(Status400BadRequest);

        return group;
    }
}
