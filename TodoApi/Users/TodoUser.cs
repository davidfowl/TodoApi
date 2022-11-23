using Microsoft.AspNetCore.Identity;

namespace TodoApi;

public class TodoUser : IdentityUser
{
    public bool IsAdmin { get; set; }
}

public class NewUser
{
    public string? Username { get; set; }
    public string? Password { get; set; }
}