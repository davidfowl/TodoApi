using System.ComponentModel.DataAnnotations;
using System.Security.Principal;

public class Todo
{
    public int Id { get; set; }
    [Required]
    public string? Title { get; set; }
    public bool IsComplete { get; set; }

    [Required]
    public string OwnerId { get; set; } = default!;
}

public class NewTodo
{
    [Required]
    public string? Title { get; set; }
}