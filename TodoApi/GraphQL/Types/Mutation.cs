using Microsoft.EntityFrameworkCore;
using TodoApi.GraphQL.Types.Todos;

namespace TodoApi.GraphQL.Types;

public class Mutation
{
    public async Task<Todo> AddTodo(TodoDbContext db, [GlobalState] CurrentUser owner, string title, CancellationToken cancellationToken)
    {
        var entity = new TodoApi.Todos.Todo
        {
            Title = title,
            OwnerId = owner.Id
        };

        await db.Todos.AddAsync(entity, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return Todo.FromEntity(entity);
    }
    
    public async Task<Todo> SetTodoIsComplete(TodoDbContext db, [GlobalState] CurrentUser owner, [ID<int>] int id, bool isComplete,  CancellationToken cancellationToken)
    {
        var entity = await db.Todos.SingleAsync(z => z.Id == id && z.OwnerId == owner.Id, cancellationToken);
        entity.IsComplete = isComplete;
        await db.SaveChangesAsync(cancellationToken);
        return Todo.FromEntity(entity);
    }
    
    public async Task<Todo> SetTodoTitleComplete(TodoDbContext db, [GlobalState] CurrentUser owner, [ID<int>] int id, string title, CancellationToken cancellationToken)
    {
        var entity = await db.Todos.SingleAsync(z => z.Id == id && z.OwnerId == owner.Id, cancellationToken);
        entity.Title = title;
        await db.SaveChangesAsync(cancellationToken);
        return Todo.FromEntity(entity);
    }
    
    public async Task<bool> DeleteTodo(TodoDbContext db, [GlobalState] CurrentUser owner, [ID<int>] int id, CancellationToken cancellationToken)
    {
        var entity = await db.Todos.SingleAsync(z => z.Id == id && z.OwnerId == owner.Id, cancellationToken);
        db.Todos.Remove(entity);
        return await db.SaveChangesAsync(cancellationToken) == 1;
    }
    
}