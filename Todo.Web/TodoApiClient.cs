using System.Collections;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;

namespace Todo.Web;

public class TodoApiClient
{
    private readonly HttpClient _httpClient;

    public TodoApiClient(HttpClient client)
    {
        _httpClient = client;
    }

    public async Task<string> GetTokenAsync(string idToken)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, "/login/Google");
        req.Headers.Authorization = new("Bearer", idToken);
        var response = await _httpClient.SendAsync(req);
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<JsonArray> GetTodosAsync(string token)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, "/todos");
        req.Headers.Authorization = new("Bearer", token);
        var response = await _httpClient.SendAsync(req);
        return (await response.Content.ReadFromJsonAsync<JsonArray>())!;
    }
}
