
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDbContext2>(o => o.UseSqlite("DataSource=Todos.db"));
var app = builder.Build();

// This makes sure the database and tables are created
var db = new TodoDbContext();
db.Database.EnsureCreated();

// TodoApi.MapRoutes(app);
// TodoApi2.MapRoutes(app);
// TodoApi3.MapRoutes(app);
TodoApi4.MapRoutes(app);

app.Run();
