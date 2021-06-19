using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Sample.Tests
{
    public class TodoTests
    {
        [Fact]
        public async Task GetTodosReturnsEmptyList()
        {
            await using var application = new TodoApplication();

            var client = application.CreateClient();
            var todos = await client.GetFromJsonAsync<List<Todo>>("/todos");

            Assert.Empty(todos);
        }

        [Fact]
        public async Task PostingNewTodoAddsItemToDatabase()
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
    }

    class TodoApplication : WebApplicationFactory<Todo>
    {
        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(sp =>
                {
                    // Replace SQL Lite with the in memory provider for tests
                    return new DbContextOptionsBuilder<TodoDbContext>()
                                .UseInMemoryDatabase("Tests")
                                .UseApplicationServiceProvider(sp)
                                .Options;
                });
            });

            return base.CreateHost(builder);
        }
    }
}
