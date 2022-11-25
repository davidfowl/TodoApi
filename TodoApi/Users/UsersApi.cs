using Microsoft.AspNetCore.Identity; 
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace TodoApi;

public static class UsersApi
{
    public static RouteGroupBuilder MapUsers(this RouteGroupBuilder group)
    {
        group.WithTags("Users");

        group.MapPost("/", async (UserInfo newUser, UserManager<TodoUser> userManager) =>
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

            return Results.ValidationProblem(result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description }));
        })
        .Produces(Status200OK)
        .ProducesValidationProblem()
        .Produces(Status400BadRequest);

        group.MapPost("/token", async (UserInfo userInfo, UserManager<TodoUser> userManager, ITokenService tokenService) =>
        {
            if (userInfo.Username is null || userInfo.Password is null)
            {
                return Results.BadRequest();
            }

            var user = await userManager.FindByNameAsync(userInfo.Username);

            if (user == null || !await userManager.CheckPasswordAsync(user, userInfo.Password))
            {
                return Results.BadRequest();
            }

            return Results.Ok(new AuthToken(tokenService.GenerateToken(user.UserName!)));
        })
        .Produces<AuthToken>()
        .Produces(Status400BadRequest);

        return group;
    }
}
