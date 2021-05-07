using System.Reflection;
using Microsoft.AspNetCore.Mvc.Routing;

class TodoApi5
{
    private readonly TodoDbContext _db;

    public TodoApi5(TodoDbContext db)
    {
        _db = db;
    }

    [HttpGet("/todos")]
    public async Task<List<Todo>> GetTodos()
    {
        return await _db.Todos.ToListAsync();
    }

    [HttpGet("/todos/{id}")]
    public async Task<IResult> GetTodo(int id)
    {
        return await _db.Todos.FindAsync(id) is Todo todo ? Ok(todo) : NotFound();
    }

    [HttpPost("/todos")]
    public async Task AddTodo(Todo todo)
    {
        await _db.Todos.AddAsync(todo);
        await _db.SaveChangesAsync();
    }

    [HttpDelete("/todos/{id}")]
    public async Task<IResult> DeleteTodo(int id)
    {
        var todo = await _db.Todos.FindAsync(id);
        if (todo is null)
        {
            return NotFound();
        }

        _db.Todos.Remove(todo);
        await _db.SaveChangesAsync();

        return Ok();
    }

    public static void MapRoutes(IEndpointRouteBuilder routes)
    {
        // This will create an optimized factory that resolves constructor arguments from the DI container
        ObjectFactory objectFactory = ActivatorUtilities.CreateFactory(typeof(TodoApi5), Type.EmptyTypes);

        foreach (var method in typeof(TodoApi5).GetMethods())
        {
            var attribute = method.GetCustomAttribute<HttpMethodAttribute>(inherit: true);

            if (attribute is null) continue;

            // Create a delegate mapping the route template and methods
            RequestDelegate del = RequestDelegateFactory.Create(method, context => objectFactory(context.RequestServices, null));

            routes.MapMethods(attribute.Template, attribute.HttpMethods, del);
        }
    }
}
