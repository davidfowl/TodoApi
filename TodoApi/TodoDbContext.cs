using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

public class TodoDbContext : DbContext
{
    public TodoDbContext(DbContextOptions options) : base(options) { }

    public DbSet<Todo> Todos { get; set; }
}

public class Todo
{
    public int Id { get; set; }
    [Required]
    public string Title { get; set; }
    public bool IsComplete { get; set; }
}