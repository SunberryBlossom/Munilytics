using JasperFx;
using JasperFx.Core;
using Microsoft.EntityFrameworkCore;
using Munilytics.Server.Domain.Entities;
using Munilytics.Server.Features.Admin.SyncFactData;
using Munilytics.Server.Features.Admin.SyncKpis;
using Munilytics.Server.Features.Admin.SyncMunicipalities;
using Munilytics.Server.Infrastructure.Persistence;
using Wolverine;
using Wolverine.Runtime.Agents;
using YamlDotNet.Serialization;

namespace Munilytics.Server.Infrastructure.Kolada
{
    public class KoladaSyncAgent : SingularAgent
    {
        private readonly ILogger<KoladaSyncAgent> _logger;
        private readonly IServiceProvider _scopeFactory;
        private Timer? _timer;

        public KoladaSyncAgent(IServiceProvider scopeFactory, ILogger<KoladaSyncAgent> logger)
            : base("kolada-sync")
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override Task startAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Sync Scheduler");

            // Check every hour if sync should run
            _timer = new Timer(async _ => await CheckState(cancellationToken), null, TimeSpan.Zero, TimeSpan.FromHours(1));
            return Task.CompletedTask;
        }

        protected override Task stopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopped Scheduler");

            _timer?.Dispose();
            return Task.CompletedTask;
        }

        public async Task CheckState(CancellationToken cancellationToken)
        {
            if (Status != AgentStatus.Running)
            {
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MunilyticsDbContext>();
            var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

            // Define all jobs here that should run weekly
            var jobs = new Dictionary<string, object>
                {
                    { "Sync.Kpis", new SyncKpiCommand() },
                    { "Sync.Municipalities", new SyncMunicipalitiesCommand() },
                    { "Sync.Fact", new SyncFactCommand() }
                };

            foreach (var job in jobs)
            {
                var key = job.Key;
                var command = job.Value;

                try
                {
                    // Check timestamp for job
                    var settings = await db.SystemSettings.FindAsync([key], cancellationToken: cancellationToken)
                        ?? new SystemSetting { Id = key, LastSync = DateTimeOffset.MinValue};
                    var timeSinceLastRun = DateTimeOffset.UtcNow - settings.LastSync;

                    // Check if there has been more than a week since last run for this job
                    if (timeSinceLastRun > TimeSpan.FromDays(7))
                    {
                        _logger.LogInformation($"Starting sync for job: {key}");

                        // Fire job and wait for it complete before we start another
                        await bus.InvokeAsync(command);

                        // Update state for latest sync
                        settings.LastSync = DateTimeOffset.UtcNow;

                        if (db.Entry(settings).State == EntityState.Detached)
                        {
                            db.Add(settings);
                        }

                        await db.SaveChangesAsync(cancellationToken);

                        _logger.LogInformation($"Finished job {key} and updated schedule");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to run {job.Key}");
                }
            }
        }
    }
}
