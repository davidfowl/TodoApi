using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace TodoApi;

public static class RateLimitExtensions
{
    private static readonly string Policy = "PerUserRatelimit";

    public static IServiceCollection AddRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter();

        // Setup defaults for the TokenBucketRateLimiterOptions and read them from config if defined
        // In theory this could be per user using named options
        services.AddOptions<TokenBucketRateLimiterOptions>()
                .Configure(options =>
                {
                    // Set defaults
                    options.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
                    options.AutoReplenishment = true;
                    options.TokenLimit = 100;
                    options.TokensPerPeriod = 100;
                    options.QueueLimit = 100;
                })
                .BindConfiguration("RateLimiting");

        // Setup the rate limiting policies taking the per user rate limiting options into account
        services.AddOptions<RateLimiterOptions>()
                .Configure((RateLimiterOptions options, IOptionsMonitor<TokenBucketRateLimiterOptions> perUserRateLimitingOptions) =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(Policy, context =>
            {
                // We always have a user name
                var username = context.User.FindFirstValue(ClaimTypes.NameIdentifier)!;

                return RateLimitPartition.GetTokenBucketLimiter(username, key =>
                {
                    return perUserRateLimitingOptions.CurrentValue;
                });
            });
        });

        return services;
    }

    public static IEndpointConventionBuilder RequirePerUserRateLimit(this IEndpointConventionBuilder builder)
    {
        return builder.RequireRateLimiting(Policy);
    }
}
