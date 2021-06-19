
var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Todos") ?? "Data Source=Todos.db";

builder.Services.AddDbContext<TodoDbContext>(o => o.UseSqlite(connectionString));

var app = builder.Build();

TodoApi2.MapRoutes(app);

app.Run();
