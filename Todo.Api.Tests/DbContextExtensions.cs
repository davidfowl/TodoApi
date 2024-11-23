namespace TodoApi.Tests;

internal static class DbContextExtensions
{
    public static IServiceCollection AddDbContextOptions<TContext>(this IServiceCollection services, Action<DbContextOptionsBuilder<TContext>> configure) where TContext : DbContext
    {
        // Remove the existing DbContextOptions
        // we want to override the settings and calling AddDbContext<TContext> again
        // will noop.
        services.RemoveAll(typeof(DbContextOptions<TContext>));

        // Add the options as singletons since the IDbContextFactory as a singleton

        // Add the DbContextOptions<TContext>
        var dbContextOptionsBuilder = new DbContextOptionsBuilder<TContext>();
        configure(dbContextOptionsBuilder);
        services.AddSingleton(dbContextOptionsBuilder.Options);

        // The untyped version just calls the typed one
        services.AddSingleton<DbContextOptions>(s => s.GetRequiredService<DbContextOptions<TContext>>());

        return services;
    }
}
