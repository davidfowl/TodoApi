
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDbContext2>(o => o.UseSqlite("DataSource=Todos.db"));
var app = builder.Build();

// TodoApi.MapRoutes(app);
// TodoApi2.MapRoutes(app);
TodoApi3.MapRoutes(app);

app.Run();
