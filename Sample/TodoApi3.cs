class TodoApi3
{
    public static void MapRoutes(IEndpointRouteBuilder routes)
    {
        async Task<List<Todo>> GetTodos([FromServices] TodoDbContext2 db)
        {
            return await db.Todos.ToListAsync();
        }

        async Task<IResult> GetTodo(int id, [FromServices] TodoDbContext2 db)
        {
            return await db.Todos.FindAsync(id) is Todo todo ? Ok(todo) : NotFound();
        }

        async Task AddTodo(Todo todo, [FromServices] TodoDbContext2 db)
        {
            await db.Todos.AddAsync(todo);
            await db.SaveChangesAsync();
        }

        async Task<IResult> DeleteTodo(int id, [FromServices] TodoDbContext2 db)
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
        routes.MapGet("/todos/{id}", GetTodo);
        routes.MapPost("/todos", AddTodo);
        routes.MapDelete("/todos/{id}", DeleteTodo);
    }
}
