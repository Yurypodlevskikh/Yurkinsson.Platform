using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace YurkinssonAuthentication.Configurations
{
    public static class RateLimitingConfiguration
    {
        public static void AddRateLimitingPolicies(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                // Global limit: 50 requests per minute for each user
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
                    context => RateLimitPartition.GetSlidingWindowLimiter("GlobalSlidingLimiter",
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 50,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 4,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0,
                    }));

                options.AddPolicy("ControllerTokenIpLimiter", httpContext =>
                RateLimitPartition.GetTokenBucketLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                    factory: _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 10, // Maximum 10 tokens
                        ReplenishmentPeriod = TimeSpan.FromMinutes(1), // Restore token once every 5 seconds
                        TokensPerPeriod = 1, // Restore 1 token per one period
                        QueueLimit = 0,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                    }));

                options.AddSlidingWindowLimiter("SingleSlidingLimiter", options =>
                {
                    options.PermitLimit = 5;
                    options.Window = TimeSpan.FromSeconds(30);
                    options.SegmentsPerWindow = 3;
                    options.QueueLimit = 0;
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
}
