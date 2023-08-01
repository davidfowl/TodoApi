using Microsoft.EntityFrameworkCore;

namespace TodoApi.GraphQL.Types;

public class Query
{
    [UseProjection]
    public IQueryable<Todos.Todo> GetTodos(TodoDbContext db, [GlobalState] CurrentUser owner)
    {
        var ownerId = owner.Id;
        return db.Todos.AsNoTracking().Where(todo => todo.OwnerId == ownerId).Select(Todos.Todo.Projection);
    }

    public async Task<Todos.Todo> GetTodo(TodoDbContext db, [GlobalState] CurrentUser owner, [ID<int>] int id, CancellationToken cancellationToken)
    {
        var ownerId = owner.Id;
        return Todos.Todo.FromEntity(await db.Todos.AsNoTracking().SingleAsync(todo => todo.OwnerId == ownerId && todo.Id == id, cancellationToken));
    }
}