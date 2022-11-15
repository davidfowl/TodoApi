using Microsoft.EntityFrameworkCore;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace TodoApi.Apis;

internal static class TodoApi
{
    public static RouteGroupBuilder MapTodos(this RouteGroupBuilder group)
    {
        group.WithTags("Todos");

        // Auth
        group.RequireJwt()
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

            await db.Todos.AddAsync(todo);
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

            if (!await db.Todos.AnyAsync(x => x.Id == id && x.OwnerId != owner.Id && !owner.IsAdmin))
            {
                return Results.NotFound();
            }

            db.Update(todo);
            await db.SaveChangesAsync();

            return Results.Ok();
        })
        .Produces(Status400BadRequest)
        .Produces(Status404NotFound)
        .Produces(Status200OK);

        group.MapDelete("/{id}", async (TodoDbContext db, int id, UserId owner) =>
        {
            var todo = await db.Todos.FindAsync(id);
            if (todo is null || todo.OwnerId != owner.Id && !owner.IsAdmin)
            {
                return Results.NotFound();
            }

            db.Todos.Remove(todo);
            await db.SaveChangesAsync();

            return Results.Ok();
        })
        .Produces(Status400BadRequest)
        .Produces(Status404NotFound)
        .Produces(Status200OK);

        return group;
    }
}
