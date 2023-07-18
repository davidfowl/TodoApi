using TodoApi.GraphQL.Interceptors;
using TodoApi.GraphQL.Types;
using TodoApi.Todos;

var builder = WebApplication.CreateBuilder(args);

// Configure auth
builder.AddAuthentication();
builder.Services.AddAuthorizationBuilder().AddCurrentUserHandler();

// Add the service to generate JWT tokens
builder.Services.AddTokenService();

// Configure the database
var connectionString = builder.Configuration.GetConnectionString("Todos") ?? "Data Source=.db/Todos.db";
builder.Services.AddSqlite<TodoDbContext>(connectionString);

// Configure identity
builder.Services.AddIdentityCore<TodoUser>()
                .AddEntityFrameworkStores<TodoDbContext>();

// State that represents the current user from the database *and* the request
builder.Services.AddCurrentUser();


builder.Services
    .AddGraphQLServer()
    .AddAuthorization()
    .AddMutationConventions()
    .AddHttpRequestInterceptor<CurrentUserInterceptor>()
    .AddProjections()
    .RegisterDbContext<TodoDbContext>()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>();

// Configure Open API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o => o.InferSecuritySchemes());

// Configure rate limiting
builder.Services.AddRateLimiting();

// Configure OpenTelemetry
builder.AddOpenTelemetry();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRateLimiter();

app.Map("/", () => Results.Redirect("/swagger"));

// Configure the APIs
app.MapGraphQL();
app.MapTodos();
app.MapUsers();

// Configure the prometheus endpoint for scraping metrics
app.MapPrometheusScrapingEndpoint();
// NOTE: This should only be exposed on an internal port!
// .RequireHost("*:9100");


app.Run();
