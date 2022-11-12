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

        var token = jwtIssuer.Create(new(
            JwtBearerDefaults.AuthenticationScheme,
            Name: Guid.NewGuid().ToString(),
            Audiences: audiences,
            Issuer: jwtIssuer.Issuer,
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