namespace Todo.Web.Server;

public class AuthClient(HttpClient client)
{
    public async Task<string?> GetTokenAsync(UserInfo userInfo)
    {
        var response = await client.PostAsJsonAsync("users/login", userInfo);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var token = await response.Content.ReadFromJsonAsync<AuthToken>();

        return token?.Token;
    }

    public async Task<string?> CreateUserAsync(UserInfo userInfo)
    {
        var response = await client.PostAsJsonAsync("users/register", userInfo);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await GetTokenAsync(userInfo);
    }

    public async Task<string?> GetOrCreateUserAsync(string provider, ExternalUserInfo userInfo)
    {
        var response = await client.PostAsJsonAsync($"users/token/{provider}", userInfo);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var token = await response.Content.ReadFromJsonAsync<AuthToken>();

        return token?.Token;
    }
}
