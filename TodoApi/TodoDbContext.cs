using System.Reflection;
using Microsoft.EntityFrameworkCore;

public class TodoDbContext : DbContext
{
    public TodoDbContext(DbContextOptions options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new TodoEntityConfiguration());
    }
    
    public DbSet<Todo> Todos => Set<Todo>();
}
