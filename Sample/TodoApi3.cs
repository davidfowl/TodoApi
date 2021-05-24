class TodoApi3
{
    public static void MapRoutes(IEndpointRouteBuilder routes)
    {
        async Task<List<Todo>> GetTodos([FromServices] TodoDbContext db)
        {
            return await db.Todos.ToListAsync();
        }

        async Task<IResult> GetTodo([FromServices] TodoDbContext db, int id)
        {
            return await db.Todos.FindAsync(id) is Todo todo ? Ok(todo) : NotFound();
        }

        async Task<IResult> AddTodo([FromServices] TodoDbContext db, Todo todo)
        {
            await db.Todos.AddAsync(todo);
            await db.SaveChangesAsync();

            return CreatedAt(todo, "todos", new { todo.Id });
        }

        async Task<IResult> DeleteTodo([FromServices] TodoDbContext db, int id)
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

        routes.MapGet("/todos", GetTodos);
        routes.MapGet("/todos/{id}", GetTodo).WithMetadata(new EndpointNameMetadata("todos"));
        routes.MapPost("/todos", AddTodo);
        routes.MapDelete("/todos/{id}", DeleteTodo);
    }
}
