using Microsoft.AspNetCore.Authentication;
using Yarp.ReverseProxy.Forwarder;

namespace Todo.Web.Server;

public static class TodoApi
{
    public static RouteGroupBuilder MapTodos(this IEndpointRouteBuilder routes, string todoUrl)
    {
        var group = routes.MapGroup("/todos");

        group.RequireAuthorization();

        var transform = static async ValueTask (HttpContext context, HttpRequestMessage req) =>
        {
            var result = await context.AuthenticateAsync();

            var properties = result.Properties!;

            var accessToken = properties.GetTokenValue(TokenNames.AccessToken);
            req.Headers.Authorization = new("Bearer", accessToken);

            if (properties.HasExternalToken() && properties.GetExternalProvider() is string externalProvider)
            {
                // Set the external provider name as the scheme so we can do auth
                // on the backend with the right configuration
                req.Headers.TryAddWithoutValidation("X-Auth-Scheme", externalProvider);
            }
        };

        // Use this HttpClient for all proxied requests
        var client = new HttpMessageInvoker(new SocketsHttpHandler());

        group.Map("{*path}", async (IHttpForwarder forwarder, HttpContext context) =>
        {
            await forwarder.SendAsync(context, todoUrl, client, transform);

            return Results.Empty;
        });

        return group;
    }
}
