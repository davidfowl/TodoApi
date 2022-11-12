using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using TodoApi.Tests;

internal class TodoApplication : WebApplicationFactory<Program>
{
    private JwtIssuer? _jwtIssuer;
    private List<string>? _audiences;

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var root = new InMemoryDatabaseRoot();

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<TodoDbContext>));

            services.AddDbContext<TodoDbContext>(options =>
                options.UseInMemoryDatabase("Testing", root));
        });

        return base.CreateHost(builder);
    }

    private string CreateToken(string id)
    {
        var configuration = Services.GetRequiredService<IConfiguration>();
        var bearerSection = configuration.GetSection("Authentication:Schemes:Bearer");
        var section = bearerSection.GetSection("SigningKeys:0");
        var issuer = section["Issuer"]!;
        var value = Convert.FromBase64String(section["Value"]!);
        _audiences = bearerSection.GetSection("ValidAudiences").GetChildren().Select(s => s.Value!).ToList();

        _jwtIssuer = new JwtIssuer(issuer, value);
        var token = _jwtIssuer!.Create(new(
            JwtBearerDefaults.AuthenticationScheme,
            Name: Guid.NewGuid().ToString(),
            Audiences: _audiences!,
            Issuer: _jwtIssuer.Issuer,
            NotBefore: DateTime.UtcNow,
            ExpiresOn: DateTime.UtcNow.AddDays(1),
            Roles: new List<string> { },
            Scopes: new List<string> { },
            Claims: new Dictionary<string, string> { ["id"] = id }));
        return JwtIssuer.WriteToken(token);
    }

    public HttpClient CreateClient(string id)
    {
        return CreateDefaultClient(new AuthHandler(req =>
        {
            var token = CreateToken(id);
            req.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, token);
        }));
    }

    private class AuthHandler : DelegatingHandler
    {
        private readonly Action<HttpRequestMessage> _onRequest;

        public AuthHandler(Action<HttpRequestMessage> onRequest)
        {
            _onRequest = onRequest;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _onRequest(request);
            return base.SendAsync(request, cancellationToken);
        }
    }
}