using Microsoft.AspNetCore.Authentication;
using Yarp.ReverseProxy.Forwarder;

namespace Todo.Web.Server;

public static class TodoApi
{
    public static RouteGroupBuilder MapTodos(this IEndpointRouteBuilder routes, string todoUrl)
    {
        var group = routes.MapGroup("/todos");

        group.RequireAuthorization();

        var invoker = new HttpMessageInvoker(new SocketsHttpHandler());

        var transform = static async ValueTask (HttpContext context, HttpRequestMessage req) =>
        {
            var result = await context.AuthenticateAsync();
            var authToken = result.Properties!.GetString("token");
            req.Headers.Authorization = new("Bearer", authToken);
        };

        group.Map("/", async (IHttpForwarder forwarder, HttpContext context) =>
        {
            await forwarder.SendAsync(context, todoUrl, invoker, transform);

            return Results.Empty;
        });

        return group;
    }
}
