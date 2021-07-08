using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Todos") ?? "Data Source=Todos.db";

builder.Services.AddDbContext<TodoDbContext>(o => o.UseSqlite(connectionString));

var app = builder.Build();

// TODO: Migrations don't work in preview5
var options = new DbContextOptionsBuilder().UseSqlite(connectionString).Options;

// This makes sure the database and tables are created
using (var db = new TodoDbContext(options))
{
    db.Database.EnsureCreated();
}

app.MapGet("/todos", async ([FromServices] TodoDbContext db) =>
{
    return await db.Todos.ToListAsync();
});

app.MapGet("/todos/{id}", async ([FromServices] TodoDbContext db, int id) =>
{
    return await db.Todos.FindAsync(id) is Todo todo ? Results.Ok(todo) : Results.NotFound();
});

app.MapPost("/todos", async ([FromServices] TodoDbContext db, Todo todo) =>
{
    await db.Todos.AddAsync(todo);
    await db.SaveChangesAsync();

    return Results.Created($"/todo/{todo.Id}", todo);
});

app.MapDelete("/todos/{id}", async ([FromServices] TodoDbContext db, int id) =>
{
    var todo = await db.Todos.FindAsync(id);
    if (todo is null)
    {
        return Results.NotFound();
    }

    db.Todos.Remove(todo);
    await db.SaveChangesAsync();

    return Results.Ok();
});

app.Run();