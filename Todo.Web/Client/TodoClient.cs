using System.Net;
using System.Net.Http.Json;

namespace Todo.Web.Client;

public class TodoClient(HttpClient client)
{
    public async Task<TodoItem?> AddTodoAsync(string? title)
    {
        if (string.IsNullOrEmpty(title))
        {
            return null;
        }

        TodoItem? createdTodo = null;

        var response = await client.PostAsJsonAsync("todos", new TodoItem { Title = title });

        if (response.IsSuccessStatusCode)
        {
            createdTodo = await response.Content.ReadFromJsonAsync<TodoItem>();
        }

        return createdTodo;
    }

    public async Task<bool> DeleteTodoAsync(int id)
    {
        var response = await client.DeleteAsync($"todos/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<(HttpStatusCode, List<TodoItem>?)> GetTodosAsync()
    {
        // This is a hack from hell to avoid having to know if this is running server or client side
        if (client.BaseAddress is null)
        {
            return (HttpStatusCode.OK, new());
        }

        var response = await client.GetAsync("todos");
        var statusCode = response.StatusCode;
        List<TodoItem>? todos = null;

        if (response.IsSuccessStatusCode)
        {
            todos = await response.Content.ReadFromJsonAsync<List<TodoItem>>();
        }

        return (statusCode, todos);
    }

    public async Task<bool> LoginAsync(string? email, string? password)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            return false;
        }

        var response = await client.PostAsJsonAsync("auth/login", new UserInfo { Email = email, Password = password });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CreateUserAsync(string? email, string? password)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            return false;
        }

        var response = await client.PostAsJsonAsync("auth/register", new UserInfo { Email = email, Password = password });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> LogoutAsync()
    {
        var response = await client.PostAsync("auth/logout", content: null);
        return response.IsSuccessStatusCode;
    }
}