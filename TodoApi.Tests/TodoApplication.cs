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
using Xunit;

internal class TodoApplication : WebApplicationFactory<Program>
{
    public TodoDbContext CreateTodoDbContext()
    {
        return Services.GetRequiredService<IDbContextFactory<TodoDbContext>>().CreateDbContext();
    }

    public HttpClient CreateClient(string id, bool isAdmin = false)
    {
        return CreateDefaultClient(new AuthHandler(req =>
        {
            var token = CreateToken(id, isAdmin);
            req.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, token);
        }));
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var root = new InMemoryDatabaseRoot();

        builder.ConfigureServices(services =>
        {
            // We need to remove this first because calling AddDbContext won't replace
            // prior calls made in the application
            services.RemoveAll(typeof(DbContextOptions<TodoDbContext>));

            // We need to make this a singleton so that we can use it from our tests
            services.AddDbContext<TodoDbContext>(options => options.UseInMemoryDatabase("Testing", root), ServiceLifetime.Singleton);
            services.AddDbContextFactory<TodoDbContext>(options => options.UseInMemoryDatabase("Testing", root));
        });

        return base.CreateHost(builder);
    }

    private string CreateToken(string id, bool isAdmin = false)
    {
        // Read the user JWTs configuration for testing so unit tests can generate
        // JWT tokens.

        var configuration = Services.GetRequiredService<IConfiguration>();
        var bearerSection = configuration.GetSection("Authentication:Schemes:Bearer");
        var section = bearerSection.GetSection("SigningKeys:0");
        var issuer = section["Issuer"];
        var signingKeyBase64 = section["Value"];

        Assert.NotNull(issuer);
        Assert.NotNull(signingKeyBase64);

        var signingKeyBytes = Convert.FromBase64String(signingKeyBase64);

        var audiences = bearerSection.GetSection("ValidAudiences").GetChildren().Select(s =>
        {
            var audience = s.Value;
            Assert.NotNull(audience);
            return audience;
        }).ToList();

        var jwtIssuer = new JwtIssuer(issuer, signingKeyBytes);

        var roles = new List<string>();

        if (isAdmin)
        {
            roles.Add("admin");
        }

        var token = jwtIssuer.Create(new(
            JwtBearerDefaults.AuthenticationScheme,
            Name: Guid.NewGuid().ToString(),
            Audiences: audiences,
            Issuer: jwtIssuer.Issuer,
            NotBefore: DateTime.UtcNow,
            ExpiresOn: DateTime.UtcNow.AddDays(1),
            Roles: roles,
            Scopes: new List<string> { },
            Claims: new Dictionary<string, string> { ["id"] = id }));

        return JwtIssuer.WriteToken(token);
    }

    private sealed class AuthHandler : DelegatingHandler
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