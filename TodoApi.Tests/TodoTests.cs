using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Sample.Migrations;
using Xunit;

namespace TodoApiTest;

public class TodoTests
{
    [Fact]
    public async Task GetTodos()
    {
        await using TodoApplication application = new();

        HttpClient client = application.CreateClient();
        List<Todo>? todos = await client.GetFromJsonAsync<List<Todo>>("/todos");

        Assert.NotNull(todos);
        Assert.Empty(todos);
    }

    [Fact]
    public async Task PostTodos()
    {
        await using TodoApplication application = new();

        HttpClient client = application.CreateClient();
        HttpResponseMessage response =
            await client.PostAsJsonAsync("/todos", new Todo { Title = "I want to do this thing tomorrow" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        List<Todo>? todos = await client.GetFromJsonAsync<List<Todo>>("/todos");

        Assert.NotNull(todos);
        Todo todo = Assert.Single(todos);
        Assert.Equal("I want to do this thing tomorrow", todo.Title);
        Assert.False(todo.IsComplete);
    }

    [Fact]
    public async Task DeleteTodos()
    {
        await using TodoApplication application = new();

        HttpClient client = application.CreateClient();
        HttpResponseMessage response =
            await client.PostAsJsonAsync("/todos", new Todo { Title = "I want to do this thing tomorrow" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        List<Todo>? todos = await client.GetFromJsonAsync<List<Todo>>("/todos");

        Assert.NotNull(todos);
        Todo todo = Assert.Single(todos);
        Assert.Equal("I want to do this thing tomorrow", todo.Title);
        Assert.False(todo.IsComplete);

        response = await client.DeleteAsync($"/todos/{todo.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        response = await client.GetAsync($"/todos/{todo.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

internal class TodoApplication : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        InMemoryDatabaseRoot root = new();

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<TodoDbContext>));

            services.AddDbContext<TodoDbContext>(options =>
                options.UseInMemoryDatabase("Testing", root));
        });

        return base.CreateHost(builder);
    }
}