using Microsoft.EntityFrameworkCore;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace TodoApi;

internal static class TodoApi
{
    public static RouteGroupBuilder MapTodos(this RouteGroupBuilder group)
    {
        group.WithTags("Todos");

        // Add security requirements, all incoming requests to this API *must*
        // be authenticated with a valid user.
        group.RequireAuthorization(pb => pb.AddIdentityUserCheck())
             .AddOpenApiSecurityRequirement();

        // Rate limit all of the APIs
        group.RequirePerUserRateLimit();

        group.MapGet("/", async (TodoDbContext db, UserId owner) =>
        {
            return await db.Todos.Where(todo => todo.OwnerId == owner.Id).ToListAsync();
        });

        group.MapGet("/{id}", async (TodoDbContext db, int id, UserId owner) =>
        {
            return await db.Todos.FindAsync(id) switch
            {
                Todo todo when todo.OwnerId == owner.Id || owner.IsAdmin => Results.Ok(todo),
                _ => Results.NotFound()
            };
        })
        .Produces<Todo>()
        .Produces(Status404NotFound);

        group.MapPost("/", async (TodoDbContext db, NewTodo newTodo, UserId owner) =>
        {
            if (string.IsNullOrEmpty(newTodo.Title))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["title"] = new[] { "A title is required" }
                });
            }

            var todo = new Todo
            {
                Title = newTodo.Title,
                OwnerId = owner.Id
            };

            db.Todos.Add(todo);
            await db.SaveChangesAsync();

            return Results.Created($"/todos/{todo.Id}", todo);
        })
       .Produces(Status201Created)
       .ProducesValidationProblem();

        group.MapPut("/{id}", async (TodoDbContext db, int id, Todo todo, UserId owner) =>
        {
            if (id != todo.Id)
            {
                return Results.BadRequest();
            }

            var rowsAffected = await db.Todos.Where(t => t.Id == id && (t.OwnerId == owner.Id || owner.IsAdmin))
                                             .ExecuteUpdateAsync(updates => 
                                                updates.SetProperty(t => t.IsComplete, todo.IsComplete)
                                                       .SetProperty(t => t.Title, todo.Title));

            if (rowsAffected == 0)
            {
                return Results.NotFound();
            }

            return Results.Ok();
        })
        .Produces(Status400BadRequest)
        .Produces(Status404NotFound)
        .Produces(Status200OK);

        group.MapDelete("/{id}", async (TodoDbContext db, int id, UserId owner) =>
        {
            var rowsAffected = await db.Todos.Where(t => t.Id == id && (t.OwnerId == owner.Id || owner.IsAdmin))
                                             .ExecuteDeleteAsync();

            if (rowsAffected == 0)
            {
                return Results.NotFound();
            }

            return Results.Ok();
        })
        .Produces(Status400BadRequest)
        .Produces(Status404NotFound)
        .Produces(Status200OK);

        return group;
    }
}
