class TodoApi2
{
    public static void MapRoutes(IEndpointRouteBuilder routes)
    {
        routes.MapGet("/todos", async ([FromServices] TodoDbContext2 db) =>
        {
            return await db.Todos.ToListAsync();
        });

        routes.MapGet("/todos/{id}", async (int id, [FromServices] TodoDbContext2 db) =>
        {
            return await db.Todos.FindAsync(id) is Todo todo ? Ok(todo) : NotFound();
        });

        routes.MapPost("/todos", async (Todo todo, [FromServices] TodoDbContext2 db) =>
        {
            await db.Todos.AddAsync(todo);
            await db.SaveChangesAsync();
        });

        routes.MapDelete("/todos/{id}", async (int id, [FromServices] TodoDbContext2 db) =>
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
