using Microsoft.AspNetCore.DataProtection;

namespace TodoApi.Tests;

internal class TodoApplication : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _sqliteConnection = new("Filename=:memory:");

    public UsersDbContext CreateTodoDbContext()
    {
        var db = Services.GetRequiredService<IDbContextFactory<UsersDbContext>>().CreateDbContext();
        db.Database.EnsureCreated();
        return db;
    }

    public async Task CreateUserAsync(string username, string? password = null)
    {
        using var scope = Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<TodoUser>>();
        var newUser = new TodoUser { Id = username };
        var result = await userManager.CreateAsync(newUser, password ?? Guid.NewGuid().ToString());
        Assert.True(result.Succeeded);
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Open the connection, this creates the SQLite in-memory database, which will persist until the connection is closed
        _sqliteConnection.Open();

        builder.ConfigureServices(services =>
        {
            // We're going to use the factory from our tests
            services.AddDbContextFactory<UsersDbContext>();

            // We need to replace the configuration for the DbContext to use a different configured database
            services.AddDbContextOptions<UsersDbContext>(o => o.UseSqlite(_sqliteConnection));

            // Lower the requirements for the tests
            services.Configure<IdentityOptions>(o =>
            {
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequireDigit = false;
                o.Password.RequiredUniqueChars = 0;
                o.Password.RequiredLength = 1;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
            });

            // Since tests run in parallel, it's possible multiple servers will startup,
            // we use an ephemeral key provider and repository to avoid filesystem contention issues
            services.AddSingleton<IDataProtectionProvider, EphemeralDataProtectionProvider>();
        });

        return base.CreateHost(builder);
    }

    protected override void Dispose(bool disposing)
    {
        _sqliteConnection?.Dispose();
        base.Dispose(disposing);
    }
}