using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Todo.Web.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddHttpClient<TodoHttpClient>(client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);

    // The cookie auth stack detects this header and avoids redirects for unauthenticated
    // requests
    client.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
});

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services
    .AddTodoClient()
    .ConfigureHttpClient(client => client.BaseAddress = new UriBuilder(builder.HostEnvironment.BaseAddress) { Path = "/graphql" }.Uri);


await builder.Build().RunAsync();
