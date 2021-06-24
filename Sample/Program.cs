using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

using static Microsoft.AspNetCore.Http.Results;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Todos") ?? "Data Source=Todos.db";

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<TodoDbContext>(o => o.UseSqlite(connectionString));
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = builder.Environment.ApplicationName, Version = "v1" });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{builder.Environment.ApplicationName} v1"));
}

app.MapGet("/todos", async (TodoDbContext db) =>
{
    return await db.Todos.ToListAsync();
});

app.MapGet("/todos/{id}", async (TodoDbContext db, int id) =>
{
    return await db.Todos.FindAsync(id) is Todo todo ? Ok(todo) : NotFound();
});

app.MapPost("/todos", async (TodoDbContext db, Todo todo) =>
{
    await db.Todos.AddAsync(todo);
    await db.SaveChangesAsync();

    return Created($"/todo/{todo.Id}", todo);
});

app.MapDelete("/todos/{id}", async (TodoDbContext db, int id) =>
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

app.Run();
