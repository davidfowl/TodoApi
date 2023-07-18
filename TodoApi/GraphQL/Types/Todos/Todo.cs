using System.Linq.Expressions;

namespace TodoApi.GraphQL.Types.Todos;

public class Todo
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public bool IsComplete { get; set; }
    public string OwnerId { get; set; } = default!;

    public static Expression<Func<TodoApi.Todos.Todo, Todo>> Projection { get; } = todo => new Todo
    {
        Id = todo.Id,
        Title = todo.Title,
        IsComplete = todo.IsComplete,
        OwnerId = todo.OwnerId
    };

    public static Func<TodoApi.Todos.Todo, Todo> FromEntity { get; } = Projection.Compile();
}