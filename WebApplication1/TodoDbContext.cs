using Microsoft.EntityFrameworkCore;

public class TodoDbContext2 : DbContext
{
    public TodoDbContext2(DbContextOptions<TodoDbContext2> options) : base(options)
    {
    }

    public DbSet<Todo> Todos { get; set; }
}

public record Todo(int Id, string Name, bool IsComplete);