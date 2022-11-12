using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.SwaggerGen;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);

// Configure auth
builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization();

// Configure the database
var connectionString = builder.Configuration.GetConnectionString("Todos") ?? "Data Source=Todos.db";
builder.Services.AddSqlite<TodoDbContext>(connectionString);

// Configure Open API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<SwaggerGeneratorOptions>(o => o.InferSecuritySchemes = true);

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Map("/", () => Results.Redirect("/swagger"));

// Configure the APIs
var group = app.MapGroup("/todos");

group.MapTodos()
     .RequireAuthorization(pb => pb.RequireClaim("id"))
     .AddOpenApiSecurityRequirement();

app.Run();
