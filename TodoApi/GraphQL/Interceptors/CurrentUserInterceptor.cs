using HotChocolate.AspNetCore;
using HotChocolate.Execution;

namespace TodoApi.GraphQL.Interceptors;

public class CurrentUserInterceptor : DefaultHttpRequestInterceptor
{
    public override ValueTask OnCreateAsync(
        HttpContext context,
        IRequestExecutor requestExecutor, 
        IQueryRequestBuilder requestBuilder,
        CancellationToken cancellationToken
    )
    {
        var user = context.RequestServices.GetService<CurrentUser>();

        if (user is not null)
        {
            requestBuilder.SetGlobalState("owner", user);
        }
        
        return base.OnCreateAsync(context, requestExecutor, requestBuilder, cancellationToken);
    }

}