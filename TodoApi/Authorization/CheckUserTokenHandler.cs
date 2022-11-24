using Microsoft.AspNetCore.Authorization;

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
        private readonly CurrentUser _currentUser;
        public CheckUserTokenHandler(CurrentUser currentUser) => _currentUser = currentUser;

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CheckUserTokenRequirement requirement)
        {
            // TODO: Check user if the user is locked out as well
            if (_currentUser.User is not null)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
