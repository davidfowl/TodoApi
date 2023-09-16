using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication().AddIdentityBearerToken<TodoUser>();

builder.Services.AddSharedKeys(builder.Configuration);

var connectionString = builder.Configuration.GetConnectionString("Users") ?? "Data Source=.db/Users.db";
builder.Services.AddSqlite<IdentityDbContext<TodoUser>>(connectionString);

// Configure Open API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o => o.AddOpenApiSecurity());

// Configure identity
builder.Services.AddIdentityCore<TodoUser>()
                .AddEntityFrameworkStores<IdentityDbContext<TodoUser>>()
                .AddApiEndpoints();

var app = builder.Build();

await MakeDb(app.Services);

async Task MakeDb(IServiceProvider sp)
{
    await using var scope = sp.CreateAsyncScope();

    var db0 = scope.ServiceProvider.GetRequiredService<SharedKeysDb>();
    var db1 = scope.ServiceProvider.GetRequiredService<IdentityDbContext<TodoUser>>();

    await db0.Database.EnsureCreatedAsync();
    await db1.Database.EnsureCreatedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapUsers();

app.Map("/", () => Results.Redirect("/swagger"));

app.Run();
