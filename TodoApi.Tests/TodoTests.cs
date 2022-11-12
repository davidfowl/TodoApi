using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Xunit;

public class TodoTests
{
    [Fact]
    public async Task GetTodos()
    {
        var userId = "34";

        await using var application = new TodoApplication();
        var client = application.CreateClient(userId);
        var todos = await client.GetFromJsonAsync<List<Todo>>("/todos");
        Assert.NotNull(todos);

        Assert.Empty(todos);
    }

    [Fact]
    public async Task PostTodos()
    {
        var userId = "34";
        await using var application = new TodoApplication();

        var client = application.CreateClient(userId);
        var response = await client.PostAsJsonAsync("/todos", new Todo { Title = "I want to do this thing tomorrow" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var todos = await client.GetFromJsonAsync<List<Todo>>("/todos");
        Assert.NotNull(todos);


        var todo = Assert.Single(todos);
        Assert.Equal("I want to do this thing tomorrow", todo.Title);
        Assert.False(todo.IsComplete);
    }

    [Fact]
    public async Task PostingTodoWithoutTitleReturnsProblemDetails()
    {
        var userId = "34";
        await using var application = new TodoApplication();

        var client = application.CreateClient(userId);
        var response = await client.PostAsJsonAsync("/todos", new Todo { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problemDetails);

        Assert.Equal("One or more validation errors occurred.", problemDetails.Title);
        Assert.NotEmpty(problemDetails.Errors);
        Assert.Equal(new[] { "A title is required" }, problemDetails.Errors["title"]);
    }

    [Fact]
    public async Task CanOnlyGetTodosPostedBySameUser()
    {
        var userId0 = "34";
        var userId1 = "35";
        await using var application = new TodoApplication();

        var client0 = application.CreateClient(userId0);
        var client1 = application.CreateClient(userId1);

        var response = await client0.PostAsJsonAsync("/todos", new Todo { Title = "I want to do this thing tomorrow" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var todos0 = await client0.GetFromJsonAsync<List<Todo>>("/todos");
        Assert.NotNull(todos0);

        var todos1 = await client1.GetFromJsonAsync<List<Todo>>("/todos");
        Assert.NotNull(todos1);

        Assert.Empty(todos1);

        var todo = Assert.Single(todos0);
        Assert.Equal("I want to do this thing tomorrow", todo.Title);
        Assert.False(todo.IsComplete);

        var todo0 = await client0.GetFromJsonAsync<Todo>($"/todos/{todo.Id}");
        Assert.NotNull(todo0);

        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => client1.GetFromJsonAsync<Todo>($"/todos/{todo.Id}"));
        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
    }

    [Fact]
    public async Task DeleteTodos()
    {
        var userId = "34";

        await using var application = new TodoApplication();

        var client = application.CreateClient(userId);
        var response = await client.PostAsJsonAsync("/todos", new Todo { Title = "I want to do this thing tomorrow" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var todos = await client.GetFromJsonAsync<List<Todo>>("/todos");
        Assert.NotNull(todos);

        var todo = Assert.Single(todos);
        Assert.Equal("I want to do this thing tomorrow", todo.Title);
        Assert.False(todo.IsComplete);

        response = await client.DeleteAsync($"/todos/{todo.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        response = await client.GetAsync($"/todos/{todo.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CannotDeleteUnownedTodos()
    {
        var userId0 = "34";
        var userId1 = "35";

        await using var application = new TodoApplication();

        var client0 = application.CreateClient(userId0);
        var client1 = application.CreateClient(userId1);

        var response = await client0.PostAsJsonAsync("/todos", new Todo { Title = "I want to do this thing tomorrow" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var todos = await client0.GetFromJsonAsync<List<Todo>>("/todos");
        Assert.NotNull(todos);

        var todo = Assert.Single(todos);
        Assert.Equal("I want to do this thing tomorrow", todo.Title);
        Assert.False(todo.IsComplete);

        response = await client1.DeleteAsync($"/todos/{todo.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var undeletedTodo = await client0.GetFromJsonAsync<Todo>($"/todos/{todo.Id}");
        Assert.NotNull(undeletedTodo);

        Assert.Equal(todo.Title, undeletedTodo.Title);
        Assert.Equal(todo.Id, undeletedTodo.Id);
    }
}
