class TodoApi4
{
    public static async Task<List<Todo>> GetTodos([FromServices] TodoDbContext db)
    {
        return await db.Todos.ToListAsync();
    }

    public static async Task<IResult> GetTodo([FromServices] TodoDbContext db, int id)
    {
        return await db.Todos.FindAsync(id) is Todo todo ? Ok(todo) : NotFound();
    }

    public static async Task AddTodo([FromServices] TodoDbContext db, Todo todo)
    {
        await db.Todos.AddAsync(todo);
        await db.SaveChangesAsync();
    }

    public static async Task<IResult> DeleteTodo([FromServices] TodoDbContext db, int id)
    {
        var todo = await db.Todos.FindAsync(id);
        if (todo is null)
        {
            return NotFound();
        }

        db.Todos.Remove(todo);
        await db.SaveChangesAsync();

        return Ok();
    }

    public static void MapRoutes(IEndpointRouteBuilder routes)
    {
        routes.MapGet("/todos", GetTodos);
        routes.MapGet("/todos/{id}", GetTodo);
        routes.MapPost("/todos", AddTodo);
        routes.MapDelete("/todos/{id}", DeleteTodo);
    }
}
