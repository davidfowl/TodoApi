class TodoApi2
{
    public static void MapRoutes(IEndpointRouteBuilder routes)
    {
        routes.MapGet("/todos", async (TodoDbContext db) =>
        {
            return await db.Todos.ToListAsync();
        });

        routes.MapGet("/todos/{id}", async (TodoDbContext db, int id) =>
        {
            return await db.Todos.FindAsync(id) is Todo todo ? Ok(todo) : NotFound();
        })
        .WithMetadata(new EndpointNameMetadata("todos"));

        routes.MapPost("/todos", async (TodoDbContext db, Todo todo) =>
        {
            await db.Todos.AddAsync(todo);
            await db.SaveChangesAsync();

            return CreatedAt(todo, "todos", new { todo.Id });
        });

        routes.MapDelete("/todos/{id}", async (TodoDbContext db, int id) =>
        {
            var todo = await db.Todos.FindAsync(id);
            if (todo is null)
            {
                return NotFound();
            }

            db.Todos.Remove(todo);
            await db.SaveChangesAsync();

            return Ok();
        });
    }
}
