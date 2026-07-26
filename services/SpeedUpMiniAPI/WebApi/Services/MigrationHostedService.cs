using DataAccess;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Services;

public class MigrationHostedService : IHostedService
{
private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MigrationHostedService> _logger;
        private const int MaxRetryCount = 5; // Maximum number of retries
        private const int RetryDelayMs = 5000; // Delay between retries

        public MigrationHostedService(IServiceProvider serviceProvider, ILogger<MigrationHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            for (int attempt = 1; attempt <= MaxRetryCount; attempt++)
            {
                try
                {
                    // _logger.LogInformation("Attempt {Attempt} to apply migration...", attempt);

                    await context.Database.MigrateAsync(cancellationToken);

                    //_logger.LogInformation("Database migrations applied successfully.");
                    return; // Exit method on success
                }
                catch (Exception)
            {
                    //_logger.LogError(ex, "Migration failed (attempt {Attmept} of {MaxRetryCount})", attempt, MaxRetryCount);

                    if (attempt < MaxRetryCount)
                    {
                        //_logger.LogWarning("Waiting {RetryDelayMs} ms before next attempt...", RetryDelayMs);
                        await Task.Delay(RetryDelayMs, cancellationToken);
                    }
                    else
                    {
                        _logger.LogCritical("Maximum retry attempts reached. Migrations were not applied.");
                    }
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
