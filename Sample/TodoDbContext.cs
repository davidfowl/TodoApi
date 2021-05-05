public class TodoDbContext : DbContext
{
    public DbSet<Todo> Todos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=Todos.db");
    }
}

/// <summary>
/// This is TodoDbContext2 is configured to be injected and configured via the dependency injection system.
/// </summary>
public class TodoDbContext2 : DbContext
{
    public TodoDbContext2(DbContextOptions<TodoDbContext2> options) : base(options)
    {
    }

    public DbSet<Todo> Todos { get; set; }
}

public record Todo(int Id, string Name, bool IsComplete);