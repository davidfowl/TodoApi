/* Shared classes can be referenced by both the Client and Server */
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class TodoItem
{
    public int Id { get; set; }
    [Required]
    public string Title { get; set; } = default!;
    public bool IsComplete { get; set; }
}

public class UserInfo
{
    [Required]
    public string Email { get; set; } = default!;

    [Required]
    public string Password { get; set; } = default!;
}

public class ExternalUserInfo
{
    [Required]
    public string Username { get; set; } = default!;

    [Required]
    public string ProviderKey { get; set; } = default!;
}

public record AuthToken([property: JsonPropertyName("accessToken")] string Token);