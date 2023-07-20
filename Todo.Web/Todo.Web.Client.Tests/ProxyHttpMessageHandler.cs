namespace Todo.Web.Client.Tests;

internal sealed class ProxyHttpMessageHandler : DelegatingHandler
{
    private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage?>> _handler;

    public ProxyHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage?>> handler)
    {
        _handler = handler;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await _handler(request, cancellationToken);
        if (response == null)
        {
            throw new InvalidOperationException("Response required");
        }
        return response;
    }

    public static Func<IServiceProvider, ProxyHttpMessageHandler> Create(Func<HttpRequestMessage, Task<HttpResponseMessage?>> handler)
    {
        return _ => new ProxyHttpMessageHandler((HttpRequestMessage request, CancellationToken _) =>
        {
            return handler(request);
        });
    }

    public static ProxyHttpMessageHandler Never(IServiceProvider _)
    {
        return new ProxyHttpMessageHandler((request, _) => Task.FromResult(default(HttpResponseMessage?)));
    }

    public static ProxyHttpMessageHandler ThrowHttpRequestException(IServiceProvider _)
    {
        return new ProxyHttpMessageHandler((request, _) => throw new HttpRequestException());
    }
}

