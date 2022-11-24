using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace TodoApi;

public static class AuthorizationHandlerExtensions
{
    public static AuthorizationBuilder AddIdentityUserCheck(this AuthorizationBuilder builder)
    {
        builder.Services.AddScoped<IAuthorizationHandler, CheckUserTokenHandler>();
        return builder;
    }

    public static AuthorizationPolicyBuilder AddIdentityUserCheck(this AuthorizationPolicyBuilder builder)
    {
        return builder.RequireAuthenticatedUser()
                      .AddRequirements(new CheckUserTokenRequirement());
    }

    private class CheckUserTokenRequirement : IAuthorizationRequirement { }

    // This authorization handler verifies that the user exists even if there's
    // a valid token
    private class CheckUserTokenHandler : AuthorizationHandler<CheckUserTokenRequirement>
    {
        private readonly UserManager<TodoUser> _userManager;
        public CheckUserTokenHandler(UserManager<TodoUser> userManager) => _userManager = userManager;

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CheckUserTokenRequirement requirement)
        {
            // The user must be authorized
            var username = context.User.Identity!.Name!;

            // Make sure that a valid token isn't enough to call the API
            var user = await _userManager.FindByNameAsync(username);

            // TODO: Check user if the user is locked out as well
            if (user is not null)
            {
                context.Succeed(requirement);
            }
        }
    }
}
