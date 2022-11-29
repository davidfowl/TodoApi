namespace TodoApi.Tests;

public class UserApiTests
{
    [Fact]
    public async Task CanCreateAUser()
    {
        await using var application = new TodoApplication();
        await using var db = application.CreateTodoDbContext();

        using var client = application.CreateClient();
        using var response = await client.PostAsJsonAsync("/users", new UserInfo { Username = "todouser", Password = "@pwd" });

        Assert.True(response.IsSuccessStatusCode);

        var user = db.Users.Single();
        Assert.NotNull(user);

        Assert.Equal("todouser", user.UserName);
    }

    [Fact]
    public async Task MissingUserOrPasswordReturnsBadRequest()
    {
        await using var application = new TodoApplication();
        await using var db = application.CreateTodoDbContext();

        using var client = application.CreateClient();
        using var missingPasswordResponse = await client.PostAsJsonAsync("/users", new UserInfo { Username = "todouser", Password = "" });

        Assert.Equal(HttpStatusCode.BadRequest, missingPasswordResponse.StatusCode);
        var content = await missingPasswordResponse.Content.ReadAsStringAsync();

        using var missingUserResponse = await client.PostAsJsonAsync("/users", new UserInfo { Username = "", Password = "password" });

        Assert.Equal(HttpStatusCode.BadRequest, missingUserResponse.StatusCode);
    }

    [Fact]
    public async Task CanGetATokenForValidUser()
    {
        await using var application = new TodoApplication();
        await using var db = application.CreateTodoDbContext();
        await application.CreateUserAsync("todouser", "p@assw0rd1");

        using var client = application.CreateClient();
        using var response = await client.PostAsJsonAsync("/users/token", new UserInfo { Username = "todouser", Password = "p@assw0rd1" });

        Assert.True(response.IsSuccessStatusCode);

        var token = await response.Content.ReadFromJsonAsync<AuthToken>();

        Assert.NotNull(token);

        // Check that the token is indeed valid

        var req = new HttpRequestMessage(HttpMethod.Get, "/todos");
        req.Headers.Authorization = new("Bearer", token.Token);
        using var responseWithToken = await client.SendAsync(req);

        Assert.True(responseWithToken.IsSuccessStatusCode);
    }

    [Fact]
    public async Task BadRequestForInvalidCredentials()
    {
        await using var application = new TodoApplication();
        await using var db = application.CreateTodoDbContext();
        await application.CreateUserAsync("todouser", "p@assw0rd1");

        using var client = application.CreateClient();
        using var response = await client.PostAsJsonAsync("/users/token", new UserInfo { Username = "todouser", Password = "prd1" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
