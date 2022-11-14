using System.ComponentModel.DataAnnotations;

public class Todo
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public bool IsComplete { get; set; }
    public string OwnerId { get; set; } = default!;
}

public class NewTodo
{
    [Required]
    public string? Title { get; set; }
}