using System.ComponentModel.DataAnnotations;

public class Todo
{
    public int Id { get; set; }
    [Required]
    public string Title { get; set; } = default!;
    public bool IsComplete { get; set; }

    [Required]
    public string OwnerId { get; set; } = default!;
}

// DTO for creating a new todo
public class NewTodo
{
    [Required]
    public string Title { get; set; } = default!;
}