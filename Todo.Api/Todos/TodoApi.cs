using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace TodoApi;

internal static class TodoApi
{
    public static RouteGroupBuilder MapTodos(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/todos");

        group.WithTags("Todos");

        // Add security requirements, all incoming requests to this API *must*
        // be authenticated with a valid user.
        group.RequireAuthorization(pb => pb.RequireCurrentUser());

        // Rate limit all of the APIs
        group.RequirePerUserRateLimit();

        // Validate the parameters
        group.WithParameterValidation(typeof(TodoItem));

        group.MapGet("/", async (TodoDbContext db, CurrentUser owner) =>
        {
            return await db.Todos.Where(todo => todo.OwnerId == owner.Id).Select(t => t.AsTodoItem()).AsNoTracking().ToListAsync();
        });

        group.MapGet("/{id}", async Task<Results<Ok<TodoItem>, NotFound>> (TodoDbContext db, int id, CurrentUser owner) =>
        {
            return await db.Todos.FindAsync(id) switch
            {
                Todo todo when todo.OwnerId == owner.Id || owner.IsAdmin => TypedResults.Ok(todo.AsTodoItem()),
                _ => TypedResults.NotFound()
            };
        });

        group.MapPost("/", async Task<Created<TodoItem>> (TodoDbContext db, TodoItem newTodo, CurrentUser owner) =>
        {
            var todo = new Todo
            {
                Title = newTodo.Title,
                OwnerId = owner.Id
            };

            db.Todos.Add(todo);
            await db.SaveChangesAsync();

            return TypedResults.Created($"/todos/{todo.Id}", todo.AsTodoItem());
        });

        group.MapPut("/{id}", async Task<Results<Ok, NotFound, BadRequest>> (TodoDbContext db, int id, TodoItem todo, CurrentUser owner) =>
        {
            if (id != todo.Id)
            {
                return TypedResults.BadRequest();
            }

            var rowsAffected = await db.Todos.Where(t => t.Id == id && (t.OwnerId == owner.Id || owner.IsAdmin))
                                             .ExecuteUpdateAsync(updates =>
                                                updates.SetProperty(t => t.IsComplete, todo.IsComplete)
                                                       .SetProperty(t => t.Title, todo.Title));

            return rowsAffected == 0 ? TypedResults.NotFound() : TypedResults.Ok();
        });

        group.MapDelete("/{id}", async Task<Results<NotFound, Ok>> (TodoDbContext db, int id, CurrentUser owner) =>
        {
            var rowsAffected = await db.Todos.Where(t => t.Id == id && (t.OwnerId == owner.Id || owner.IsAdmin))
                                             .ExecuteDeleteAsync();

            return rowsAffected == 0 ? TypedResults.NotFound() : TypedResults.Ok();
        });

        return group;
    }
}
