using Microsoft.EntityFrameworkCore;

namespace Sample.Migrations;

public class TodoDbContext : DbContext
{
    public DbSet<Todo> Todos => Set<Todo>();

    public TodoDbContext(DbContextOptions options) : base(options)
    {
    }
}