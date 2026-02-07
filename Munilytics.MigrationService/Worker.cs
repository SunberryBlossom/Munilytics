using Microsoft.EntityFrameworkCore;
using Munilytics.Server.Infrastructure.Persistence;

namespace Munilytics.MigrationService
{
    public class Worker(ILogger<Worker> logger, IServiceProvider serviceProvider, IHostApplicationLifetime hostApplicationLifetime) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("Starting database migrations");

                using var scope = serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<MunilyticsDbContext>();

                await dbContext.Database.MigrateAsync(stoppingToken);

                logger.LogInformation("Finished database migrations!");

                hostApplicationLifetime.StopApplication();
            }
        }
    }
}
