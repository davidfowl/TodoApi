using Microsoft.AspNetCore.Components.Authorization;

internal class HttpAuthenticationStateProvider(IHttpContextAccessor httpContextAccessor) : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(new AuthenticationState(httpContextAccessor.HttpContext?.User ?? new()));
    }
}