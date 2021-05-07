using System.ComponentModel.DataAnnotations;

public class TodoDbContext : DbContext
{
    public TodoDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Todo> Todos { get; set; }
}

public record Todo(int Id, [property: Required] string Title, bool IsComplete);