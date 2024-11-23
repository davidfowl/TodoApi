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

// The DTO that excludes the OwnerId (we don't want that exposed to clients)
public class TodoItem
{
    public int Id { get; set; }
    [Required]
    public string Title { get; set; } = default!;
    public bool IsComplete { get; set; }
}

public static class TodoMappingExtensions
{
    public static TodoItem AsTodoItem(this Todo todo)
    {
        return new()
        {
            Id = todo.Id,
            Title = todo.Title,
            IsComplete = todo.IsComplete,
        };
    }
}