using Microsoft.AspNetCore.Authentication;
using Yarp.ReverseProxy.Forwarder;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace Todo.Web.Server;

public static class TodoApi
{
    public static RouteGroupBuilder MapTodos(this IEndpointRouteBuilder routes, string todoUrl)
    {
        var group = routes.MapGroup("/todos");

        group.RequireAuthorization();

        var transformBuilder = routes.ServiceProvider.GetRequiredService<ITransformBuilder>();
        var transform = transformBuilder.Create(b =>
        {
            b.AddRequestTransform(async c =>
            {
                var accessToken = await c.HttpContext.GetTokenAsync(TokenNames.AccessToken);

                c.ProxyRequest.Headers.Authorization = new("Bearer", accessToken);
            });
        });

        group.MapForwarder("{*path}", todoUrl, new ForwarderRequestConfig(), transform);

        return group;
    }
}
