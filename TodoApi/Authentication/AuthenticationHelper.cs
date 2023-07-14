using System.Security.Claims;

namespace TodoApi;

public class AuthenticationConstants
{
    public static readonly string BearerTokenScheme = "Bearer";
}

public class AuthenticationHelper
{
    public static ClaimsPrincipal CreateClaimsPrincipal(string userName, bool isAdmin = false)
    {
        var identity = new ClaimsIdentity(AuthenticationConstants.BearerTokenScheme);

        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userName));

        if (isAdmin)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, "admin"));
        }

        return new ClaimsPrincipal(identity);
    }
}
