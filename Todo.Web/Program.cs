using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Todo.Web;

var builder = WebApplication.CreateBuilder(args);

// Auth
builder.AddAuthentication();

string todoApiUrl = builder.Configuration["TodoApi:Url"] ?? throw new InvalidOperationException("Missing todo api configuration");

builder.Services.AddHttpClient<TodoApiClient>(client =>
{
    client.BaseAddress = new Uri(todoApiUrl);
});

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Login endpoints
var group = app.MapGroup("/account");

group.MapGet("/login", context => context.ChallengeAsync("Google", new() { RedirectUri = "/" }));
group.MapPost("/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/");
});

app.MapRazorPages();

app.Run();
