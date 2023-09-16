using Todo.Web.Client;
using Todo.Web.Server;

var builder = WebApplication.CreateBuilder(args);

// Configure auth with the front end
builder.AddAuthentication();
builder.Services.AddAuthorizationBuilder();

// Must add client services
builder.Services.AddScoped<TodoClient>();

builder.Services.AddRazorComponents()
                .AddWebAssemblyComponents();

// Add the forwarder to make sending requests to the backend easier
builder.Services.AddHttpForwarder();

var todoUrl = builder.Configuration.GetServiceUri("todoapi")?.ToString() ??
              builder.Configuration["TodoApiUrl"] ?? 
              throw new InvalidOperationException("Todo API URL is not configured");

var identityUrl = builder.Configuration.GetServiceUri("identityapi")?.ToString() ??
                  builder.Configuration["IdentityApiUrl"] ??
                  throw new InvalidOperationException("Todo API URL is not configured");

// Configure the HttpClient for the backend API
builder.Services.AddHttpClient<AuthClient>(client =>
{
    client.BaseAddress = new(identityUrl);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.MapRazorComponents<App>()
   .AddWebAssemblyRenderMode();

// Configure the APIs
app.MapAuth();
app.MapTodos(todoUrl);

app.Run();

