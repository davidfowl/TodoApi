var builder = WebApplication.CreateBuilder(args);

// Configure auth
builder.Services.AddAuthentication().AddBearerToken("Identity.Bearer");
builder.Services.AddAuthorizationBuilder().AddCurrentUserHandler();

builder.Services.AddSharedKeys(builder.Configuration);

// Configure the database
var connectionString = builder.Configuration.GetConnectionString("Todos") ?? "Data Source=.db/Todos.db";
builder.Services.AddSqlite<TodoDbContext>(connectionString);

// State that represents the current user from the database *and* the request
builder.Services.AddCurrentUser();

// Configure Open API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o => o.AddOpenApiSecurity());

// Configure rate limiting
builder.Services.AddRateLimiting();

// Configure OpenTelemetry
builder.AddOpenTelemetry();

var app = builder.Build();

await MakeDb(app.Services);

async Task MakeDb(IServiceProvider sp)
{
    await using var scope = sp.CreateAsyncScope();

    var db0 = scope.ServiceProvider.GetRequiredService<TodoDbContext>();

    await db0.Database.EnsureCreatedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRateLimiter();

app.Map("/", () => Results.Redirect("/swagger"));

// Configure the APIs
app.MapTodos();

// Configure the prometheus endpoint for scraping metrics
app.MapPrometheusScrapingEndpoint();
// NOTE: This should only be exposed on an internal port!
// .RequireHost("*:9100");

app.Run();
