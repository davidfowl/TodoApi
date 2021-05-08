
var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Todos") ?? "Data Source=Todos.db";

// Uncomment this to use dependency injection
// builder.Services.AddDbContext<TodoDbContext>(o => o.UseSqlite(connectionString));

var app = builder.Build();

var options = new DbContextOptionsBuilder().UseSqlite(connectionString).Options;

// This makes sure the database and tables are created
using (var db = new TodoDbContext(options))
{
    db.Database.EnsureCreated();
}

// Register the routes
TodoApi.MapRoutes(app, options);

app.Run();
