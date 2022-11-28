namespace Todo.Web.Server;

public class TodoClient
{
    private readonly HttpClient _client;

    public TodoClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<string?> GetTokenAsync(UserInfo userInfo)
    {
        var response = await _client.PostAsJsonAsync("users/token", userInfo);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var token = await response.Content.ReadFromJsonAsync<AuthToken>();

        return token?.Token;
    }

    public async Task<string?> CreateUserAsync(UserInfo userInfo)
    {
        var response = await _client.PostAsJsonAsync("users", userInfo);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        // TODO: Make this a single call on the backend
        return await GetTokenAsync(userInfo);
    }
}
