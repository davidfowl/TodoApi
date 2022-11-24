using Microsoft.AspNetCore.Identity;

namespace TodoApi;

public static class CurrentUserExtensions
{
    public static IServiceCollection AddCurrentUser(this IServiceCollection services)
    {
        return services.AddScoped<CurrentUser>();
    }

    public static IApplicationBuilder UseCurrentUser(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            // Resolve the current user so we can set the properties on this scoped instance
            var currentUser = context.RequestServices.GetRequiredService<CurrentUser>();

            currentUser.Principal = context.User;

            if (currentUser.Id is { } id)
            {
                // Resolve the user manager and see if the current user is a valid user in the database
                // we do this once and store it on the current user.
                var userManager = context.RequestServices.GetRequiredService<UserManager<TodoUser>>();
                currentUser.User = await userManager.FindByNameAsync(id);
            }

            await next(context);
        });
    }
}