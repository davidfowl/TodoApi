using Microsoft.EntityFrameworkCore;

namespace TodoApi;

public class TodoDbContext(DbContextOptions<TodoDbContext> options) : DbContext(options)
{
    public DbSet<Todo> Todos => Set<Todo>();

    public DbSet<TodoUser> Users => Set<TodoUser>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Todo>()
               .HasOne<TodoUser>()
               .WithMany()
               .HasForeignKey(t => t.OwnerId)
               .HasPrincipalKey(u => u.Id);

        base.OnModelCreating(builder);
    }
}
