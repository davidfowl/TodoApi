using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Sample.Tests
{
    public class TodoTests
    {
        [Fact]
        public async Task GetTodos()
        {
            await using var application = new TodoApplication();

            var client = application.CreateClient();
            var todos = await client.GetFromJsonAsync<List<Todo>>("/todos");

            Assert.Empty(todos);
        }

        [Fact]
        public async Task PostTodos()
        {
            await using var application = new TodoApplication();

            var client = application.CreateClient();
            var response = await client.PostAsJsonAsync("/todos", new Todo { Title = "I want to do this thing tomorrow" });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var todos = await client.GetFromJsonAsync<List<Todo>>("/todos");

            Assert.Single(todos);
            Assert.Equal("I want to do this thing tomorrow", todos[0].Title);
            Assert.False(todos[0].IsComplete);
        }

        [Fact]
        public async Task DeleteTodos()
        {
            await using var application = new TodoApplication();

            var client = application.CreateClient();
            var response = await client.PostAsJsonAsync("/todos", new Todo { Title = "I want to do this thing tomorrow" });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var todos = await client.GetFromJsonAsync<List<Todo>>("/todos");

            Assert.Single(todos);
            Assert.Equal("I want to do this thing tomorrow", todos[0].Title);
            Assert.False(todos[0].IsComplete);

            await client.DeleteAsync($"/todos/{todos[0].Id}");

            response = await client.GetAsync($"/todos/{todos[0].Id}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }

    class TodoApplication : WebApplicationFactory<Todo>
    {
        protected override IHost CreateHost(IHostBuilder builder)
        {
            var root = new InMemoryDatabaseRoot();

            builder.ConfigureServices(services => 
            {
                services.AddScoped(sp =>
                {
                    // Replace SQL Lite with the in memory provider for tests
                    return new DbContextOptionsBuilder<TodoDbContext>()
                                .UseInMemoryDatabase("Tests", root)
                                .UseApplicationServiceProvider(sp)
                                .Options;
                });
            });

            return base.CreateHost(builder);
        }
    }
}
