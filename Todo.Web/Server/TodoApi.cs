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
            var accessToken = await context.GetTokenAsync(TokenNames.AccessToken);
            req.Headers.Authorization = new("Bearer", accessToken);
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
