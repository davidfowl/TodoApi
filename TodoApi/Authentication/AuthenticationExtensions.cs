namespace TodoApi;

public static class AuthenticationExtensions
{
    public static WebApplicationBuilder AddAuthentication(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication().AddBearerToken(o =>
        {
            o.Events = new()
            {
                OnMessageReceived = context =>
                {
                    return Task.CompletedTask;
                }
            };
        });

        return builder;
    }
}
