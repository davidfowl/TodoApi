namespace TodoApi.Tests;

public class TodoApiTests
{
    [Fact]
    public async Task GetTodos()
    {
        var userId = "34";

        await using var application = new TodoApplication();
        await using var db = application.CreateTodoDbContext();
        await application.CreateUserAsync(userId);

        db.Todos.Add(new Todo { Title = "Thing one I have to do", OwnerId = userId });

        await db.SaveChangesAsync();

        var client = application.CreateClient(userId);
        var todos = await client.GetFromJsonAsync<List<TodoItem>>("/todos");
        Assert.NotNull(todos);

        var todo = Assert.Single(todos);
        Assert.Equal("Thing one I have to do", todo.Title);
    }

    [Fact]
    public async Task GetTodosWithoutDbUser()
    {
        var userId = "34";

        await using var application = new TodoApplication();
        await using var db = application.CreateTodoDbContext();

        var client = application.CreateClient(userId);
        var response = await client.GetAsync("/todos");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task PostTodos()
    {
        var userId = "34";
        await using var application = new TodoApplication();
        await using var db = application.CreateTodoDbContext();
        await application.CreateUserAsync(userId);

        var client = application.CreateClient(userId);
        var response = await client.PostAsJsonAsync("/todos", new TodoItem { Title = "I want to do this thing tomorrow" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var todos = await client.GetFromJsonAsync<List<TodoItem>>("/todos");
        Assert.NotNull(todos);

        var todo = Assert.Single(todos);
        Assert.Equal("I want to do this thing tomorrow", todo.Title);
        Assert.False(todo.IsComplete);
    }

    [Fact]
    public async Task DeleteTodos()
    {
        var userId = "34";

        await using var application = new TodoApplication();
        await using var db = application.CreateTodoDbContext();
        await application.CreateUserAsync(userId);

        db.Todos.Add(new Todo { Title = "I want to do this thing tomorrow", OwnerId = userId });

        await db.SaveChangesAsync();

        var client = application.CreateClient(userId);

        var todo = db.Todos.FirstOrDefault();
        Assert.NotNull(todo);
        Assert.Equal("I want to do this thing tomorrow", todo.Title);
        Assert.False(todo.IsComplete);

        var response = await client.DeleteAsync($"/todos/{todo.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        todo = db.Todos.FirstOrDefault();
        Assert.Null(todo);
    }

    [Fact]
    public async Task CanOnlyGetTodosPostedBySameUser()
    {
        var userId0 = "34";
        var userId1 = "35";

        await using var application = new TodoApplication();
        await using var db = application.CreateTodoDbContext();
        await application.CreateUserAsync(userId0);
        await application.CreateUserAsync(userId1);

        db.Todos.Add(new Todo { Title = "I want to do this thing tomorrow", OwnerId = userId0 });

        await db.SaveChangesAsync();

        var client0 = application.CreateClient(userId0);
        var client1 = application.CreateClient(userId1);

        var todos0 = await client0.GetFromJsonAsync<List<TodoItem>>("/todos");
        Assert.NotNull(todos0);

        var todos1 = await client1.GetFromJsonAsync<List<TodoItem>>("/todos");
        Assert.NotNull(todos1);

        Assert.Empty(todos1);

        var todo = Assert.Single(todos0);
        Assert.Equal("I want to do this thing tomorrow", todo.Title);
        Assert.False(todo.IsComplete);

        var todo0 = await client0.GetFromJsonAsync<TodoItem>($"/todos/{todo.Id}");
        Assert.NotNull(todo0);

        var response = await client1.GetAsync($"/todos/{todo.Id}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostingTodoWithoutTitleReturnsProblemDetails()
    {
        var userId = "34";
        await using var application = new TodoApplication();
        await using var db = application.CreateTodoDbContext();
        await application.CreateUserAsync(userId);

        var client = application.CreateClient(userId);
        var response = await client.PostAsJsonAsync("/todos", new TodoItem { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problemDetails);

        Assert.Equal("One or more validation errors occurred.", problemDetails.Title);
        Assert.NotEmpty(problemDetails.Errors);
        Assert.Equal(new[] { "The Title field is required." }, problemDetails.Errors["Title"]);
    }

    [Fact]
    public async Task CannotDeleteUnownedTodos()
    {
        var userId0 = "34";
        var userId1 = "35";

        await using var application = new TodoApplication();
        await using var db = application.CreateTodoDbContext();
        await application.CreateUserAsync(userId0);
        await application.CreateUserAsync(userId1);

        db.Todos.Add(new Todo { Title = "I want to do this thing tomorrow", OwnerId = userId0 });

        await db.SaveChangesAsync();

        var client0 = application.CreateClient(userId0);
        var client1 = application.CreateClient(userId1);

        var todos = await client0.GetFromJsonAsync<List<TodoItem>>("/todos");
        Assert.NotNull(todos);

        var todo = Assert.Single(todos);
        Assert.Equal("I want to do this thing tomorrow", todo.Title);
        Assert.False(todo.IsComplete);

        var response = await client1.DeleteAsync($"/todos/{todo.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var undeletedTodo = db.Todos.FirstOrDefault();
        Assert.NotNull(undeletedTodo);

        Assert.Equal(todo.Title, undeletedTodo.Title);
        Assert.Equal(todo.Id, undeletedTodo.Id);
    }

    [Fact]
    public async Task AdminCanDeleteUnownedTodos()
    {
        var userId = "34";
        var adminUserId = "35";

        await using var application = new TodoApplication();
        await using var db = application.CreateTodoDbContext();
        await application.CreateUserAsync(userId);
        await application.CreateUserAsync(adminUserId);

        db.Todos.Add(new Todo { Title = "I want to do this thing tomorrow", OwnerId = userId });

        await db.SaveChangesAsync();

        var client = application.CreateClient(userId);
        var adminClient = application.CreateClient(adminUserId, isAdmin: true);

        var todos = await client.GetFromJsonAsync<List<TodoItem>>("/todos");
        Assert.NotNull(todos);

        var todo = Assert.Single(todos);
        Assert.Equal("I want to do this thing tomorrow", todo.Title);
        Assert.False(todo.IsComplete);

        var response = await adminClient.DeleteAsync($"/todos/{todo.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var undeletedTodo = db.Todos.FirstOrDefault();
        Assert.Null(undeletedTodo);
    }

    /// <summary>
    /// Verify that user can only update owned Todos
    /// Change userId
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task CanUpdateOwnedTodos()
    {
        var ownerId = "34";
        var userId = "34";
        await using var application = new TodoApplication();
        await using var db = application.CreateTodoDbContext();
        await application.CreateUserAsync(userId);

        db.Todos.Add(new Todo { Title = "I want to do this thing tomorrow", OwnerId = ownerId });

        await db.SaveChangesAsync();

        // Create API Client
        var client = application.CreateClient(userId);

        var todos = await client.GetFromJsonAsync<List<TodoItem>>("/todos");

        Assert.NotNull(todos);

        var todo = Assert.Single(todos);

        //update the status
        todo.IsComplete = true;

        var response = await client.PutAsJsonAsync($"todos/{todo.Id}", todo);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify the update
        todos = await client.GetFromJsonAsync<List<TodoItem>>("/todos");
        Assert.NotNull(todos);
        var updatedTodo = Assert.Single(todos);
        Assert.NotNull(updatedTodo);
        Assert.True(updatedTodo.IsComplete);

    }

    /// <summary>
    /// Check if Admin can update any todo item
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task AdminCanUpdateUnownedTodos()
    {
        var userId = "34";
        var adminUserId = "35";

        await using var application = new TodoApplication();
        await using var db = application.CreateTodoDbContext();
        await application.CreateUserAsync(userId);
        await application.CreateUserAsync(adminUserId);

        db.Todos.Add(new Todo { Title = "I want to do this thing tomorrow", OwnerId = userId });

        await db.SaveChangesAsync();

        var client = application.CreateClient(userId);
        var adminClient = application.CreateClient(adminUserId, isAdmin: true);

        var todos = await client.GetFromJsonAsync<List<TodoItem>>("/todos");
        Assert.NotNull(todos);

        var todo = Assert.Single(todos);

        //Update the todo
        todo.IsComplete = true;

        var response = await adminClient.PutAsJsonAsync($"/todos/{todo.Id}", todo);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify the changes
        todos = await client.GetFromJsonAsync<List<TodoItem>>("/todos");
        Assert.NotNull(todos);
        var updatedTodo = Assert.Single(todos);
        Assert.NotNull(updatedTodo);
        Assert.True(updatedTodo.IsComplete);
    }
}
