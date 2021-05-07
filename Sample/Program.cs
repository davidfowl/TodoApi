
var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Todos") ?? "Data Source=Todos.db";

// Uncomment this to use dependency injection
// builder.Services.AddDbContext<TodoDbContext>(o => o.UseSqlite(connectionString));

var app = builder.Build();

var options = new DbContextOptionsBuilder().UseSqlite(connectionString).Options;

// This makes sure the database and tables are created
{
    using var db = new TodoDbContext(options);
    db.Database.EnsureCreated();
}

TodoApi.MapRoutes(app, options);
// TodoApi2.MapRoutes(app);
// TodoApi3.MapRoutes(app);
// TodoApi4.MapRoutes(app);

app.Run();
