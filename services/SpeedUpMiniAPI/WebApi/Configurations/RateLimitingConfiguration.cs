using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace WebApi.Configuration;

public static class RateLimitingConfiguration
{
    public static void AddRateLimitingPolicies(this IServiceCollection services)
    {
        services.AddRateLimiter(options => 
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
                context => RateLimitPartition.GetSlidingWindowLimiter("GlobalSlidingLimiter", 
                _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1),
                    SegmentsPerWindow = 4,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 5
                })
            );

            options.AddPolicy("IdentityLimiter", HttpContext =>
            RateLimitPartition.GetTokenBucketLimiter(
                    partitionKey: HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                    factory: _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 20,
                        ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                        TokensPerPeriod = 5,
                        QueueLimit = 5,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                    }));

            options.AddSlidingWindowLimiter("SettingsLimiter", options =>
            {
                options.PermitLimit = 50;
                options.Window = TimeSpan.FromSeconds(30);
                options.SegmentsPerWindow = 3;
                options.QueueLimit = 10;
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });

            // Handling rejected requests
            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                //context.HttpContext.Response.Headers["Retry-After"] = "60";
                await context.HttpContext.Response.WriteAsync(
                    "Too many requests. Please try again later.", cancellationToken);
            };
        });
    }
}