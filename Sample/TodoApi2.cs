class TodoApi2
{
    public static void MapRoutes(IEndpointRouteBuilder routes)
    {
        routes.MapGet("/todos", async ([FromServices] TodoDbContext db) =>
        {
            return await db.Todos.ToListAsync();
        });

        routes.MapGet("/todos/{id}", async ([FromServices] TodoDbContext db, int id) =>
        {
            return await db.Todos.FindAsync(id) is Todo todo ? Ok(todo) : NotFound();
        });

        routes.MapPost("/todos", async ([FromServices] TodoDbContext db, Todo todo) =>
        {
            await db.Todos.AddAsync(todo);
            await db.SaveChangesAsync();
        });

        routes.MapDelete("/todos/{id}", async ([FromServices] TodoDbContext db, int id) =>
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
