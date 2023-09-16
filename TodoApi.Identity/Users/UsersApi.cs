using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;

namespace TodoApi;

public static class UsersApi
{
    public static RouteGroupBuilder MapUsers(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/users");

        group.WithTags("Users");

        group.WithParameterValidation(typeof(ExternalUserInfo));

        group.MapIdentityApi<TodoUser>();

        // The MapIdentityApi<T> doesn't expose an external login endpoint so we write this custom endpoint that follows
        // a similar pattern
        group.MapPost("/token/{provider}", async Task<Results<SignInHttpResult, ValidationProblem>> (string provider, ExternalUserInfo userInfo, UserManager<TodoUser> userManager, SignInManager<TodoUser> signInManager) =>
        {
            var user = await userManager.FindByLoginAsync(provider, userInfo.ProviderKey);

            var result = IdentityResult.Success;

            if (user is null)
            {
                user = new TodoUser() { UserName = userInfo.Username };

                result = await userManager.CreateAsync(user);

                if (result.Succeeded)
                {
                    result = await userManager.AddLoginAsync(user, new UserLoginInfo(provider, userInfo.ProviderKey, displayName: null));
                }
            }

            if (result.Succeeded)
            {
                var principal = await signInManager.CreateUserPrincipalAsync(user);

                return TypedResults.SignIn(principal);
            }

            return TypedResults.ValidationProblem(result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description }));
        })
        // Add the open API response for 200 since AccessTokenResponse
        // is internal and we don't want to duplicate it.
        .Produces("AccessTokenResponse");

        return group;
    }
}
