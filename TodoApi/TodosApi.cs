using Microsoft.EntityFrameworkCore;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace TodoApi;

internal static class TodosApi
{
    public static RouteGroupBuilder MapTodos(this RouteGroupBuilder group)
    {
        group.WithTags("Todos");

        group.MapGet("/", async (TodoDbContext db) =>
        {
            return await db.Todos.ToListAsync();
        });

        group.MapGet("/{id}", async (TodoDbContext db, int id) =>
        {
            return await db.Todos.FindAsync(id) switch
            {
                Todo todo => Results.Ok(todo),
                null => Results.NotFound()
            };
        })
        .Produces<Todo>()
        .Produces(Status404NotFound);

        group.MapPost("/", async (TodoDbContext db, Todo todo) =>
        {
            await db.Todos.AddAsync(todo);
            await db.SaveChangesAsync();

            return Results.Created($"/todos/{todo.Id}", todo);
        })
       .Produces(Status201Created);

        group.MapPut("/{id}", async (TodoDbContext db, int id, Todo todo) =>
        {
            if (id != todo.Id)
            {
                return Results.BadRequest();
            }

            if (!await db.Todos.AnyAsync(x => x.Id == id))
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

        group.MapDelete("/{id}", async (TodoDbContext db, int id) =>
        {
            var todo = await db.Todos.FindAsync(id);
            if (todo is null)
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
