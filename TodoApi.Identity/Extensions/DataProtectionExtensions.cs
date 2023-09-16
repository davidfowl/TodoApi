using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TodoApi;

public static class DataProtectionExtensions
{
    public static IServiceCollection AddSharedKeys(this IServiceCollection services, IConfiguration configuration)
    {
        var keysConnectionString = configuration.GetConnectionString("Keys") ?? "Data Source=../Keys.db";
        services.AddSqlite<SharedKeysDb>(keysConnectionString);

        services.AddDataProtection()
                .SetApplicationName("TodoApp")
                .PersistKeysToDbContext<SharedKeysDb>();

        return services;
    }
}

public class SharedKeysDb(DbContextOptions<SharedKeysDb> options) : DbContext(options), IDataProtectionKeyContext
{
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();
}
