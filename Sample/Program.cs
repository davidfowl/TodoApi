
var builder = WebApplication.CreateBuilder(args);
// Uncomment this to use dependency injection
// builder.Services.AddDbContext<TodoDbContext>(o => o.UseSqlite("DataSource=Todos.db"));
var app = builder.Build();

var options = new DbContextOptionsBuilder().UseSqlite("Data Source=Todos.db").Options;

// This makes sure the database and tables are created
var db = new TodoDbContext(options);
db.Database.EnsureCreated();

TodoApi.MapRoutes(app, options);
// TodoApi2.MapRoutes(app);
// TodoApi3.MapRoutes(app);
// TodoApi4.MapRoutes(app);

app.Run();
