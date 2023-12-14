﻿using Microsoft.AspNetCore.Authentication;
using Yarp.ReverseProxy.Forwarder;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace Todo.Web.Server;

public static class TodoApi
{
    public static RouteGroupBuilder MapTodos(this IEndpointRouteBuilder routes, string todoUrl)
    {
        // The todo API translates the authentication cookie between the browser the BFF into an 
        // access token that is sent to the todo API. We're using YARP to forward the request.

        var group = routes.MapGroup("/todos");

        group.RequireAuthorization();

        // Proxy with auth and path transform
        group.MapForwarder("{*path}", todoUrl, new ForwarderRequestConfig(), b =>
        {
            b.AddRequestTransform(async c =>
            {
                var accessToken = await c.HttpContext.GetTokenAsync(TokenNames.AccessToken);
                
                c.ProxyRequest.Headers.Authorization = new("Bearer", accessToken);
            });
        });


        // TODO: Remove when new YARP is released, see https://github.com/microsoft/reverse-proxy/issues/2131
        // var transformBuilder = routes.ServiceProvider.GetRequiredService<ITransformBuilder>();
        // var transform = transformBuilder.Create(b =>
        // {
        //     b.AddRequestTransform(async c =>
        //     {
        //         var accessToken = await c.HttpContext.GetTokenAsync(TokenNames.AccessToken);

        //         c.ProxyRequest.Headers.Authorization = new("Bearer", accessToken);
        //     });
        // });

        // group.MapForwarder("{*path}", todoUrl, new ForwarderRequestConfig(), transform);

        return group;
    }
}
