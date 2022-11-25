using Microsoft.AspNetCore.Identity;

namespace TodoApi;

public class TodoUser : IdentityUser
{

}

public class UserInfo
{
    public string? Username { get; set; }
    public string? Password { get; set; }
}