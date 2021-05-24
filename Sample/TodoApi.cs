class TodoApi
{
    public static void MapRoutes(IEndpointRouteBuilder routes, DbContextOptions options)
    {
        routes.MapGet("/todos", async () =>
        {
            using var db = new TodoDbContext(options);
            return await db.Todos.ToListAsync();
        });

        routes.MapGet("/todos/{id}", async (int id) =>
        {
            using var db = new TodoDbContext(options);
            return await db.Todos.FindAsync(id) is Todo todo ? Ok(todo) : NotFound();
        })
        .WithMetadata(new EndpointNameMetadata("todos"));

        routes.MapPost("/todos", async (Todo todo) =>
        {
            using var db = new TodoDbContext(options);
            await db.Todos.AddAsync(todo);
            await db.SaveChangesAsync();

            return CreatedAt(todo, "todos", new { todo.Id });
        });

        routes.MapDelete("/todos/{id}", async (int id) =>
        {
            using var db = new TodoDbContext(options);
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
