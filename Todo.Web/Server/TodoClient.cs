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

        if (token is null)
        {
            return null;
        }

        return token.Token;
    }
}
