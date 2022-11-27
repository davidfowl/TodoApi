using System.Net;
using System.Net.Http.Json;

namespace Todo.Web.Client;

public class TodoClient
{
    private readonly HttpClient _client;
    public TodoClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<bool> AddTodoAsync(string? title)
    {
        if (string.IsNullOrEmpty(title))
        {
            return false;
        }

        var response = await _client.PostAsJsonAsync("todos", new TodoItem { Title = title });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteTodoAsync(int id)
    {
        var response = await _client.DeleteAsync($"todos/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<(HttpStatusCode, TodoItem[]?)> GetTodosAsync()
    {
        var response = await _client.GetAsync("todos");
        var statusCode = response.StatusCode;
        TodoItem[]? todos = null;

        if (response.IsSuccessStatusCode)
        {
            todos = await response.Content.ReadFromJsonAsync<TodoItem[]>();
        }

        return (statusCode, todos);
    }

    public async Task<bool> LoginAsync(string? username, string? password)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            return false;
        }

        var response = await _client.PostAsJsonAsync("auth/login", new UserInfo { Username = username, Password = password });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CreateUserAsync(string? username, string? password)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            return false;
        }

        var response = await _client.PostAsJsonAsync("auth/register", new UserInfo { Username = username, Password = password });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> LogoutAsync()
    {
        var response = await _client.PostAsync("auth/logout", content: null);
        return response.IsSuccessStatusCode;
    }
}