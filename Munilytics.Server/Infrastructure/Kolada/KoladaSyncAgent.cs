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
            var jobs = new Dictionary<string, Func<Task<object>>>
                {
                    { "Sync.Kpis", () => Task.FromResult<object>(new SyncKpiCommand()) },
                    { "Sync.Municipalities", () => Task.FromResult<object>(new SyncMunicipalitiesCommand()) },
                    {
                        "Sync.Fact",
                        async () =>
                        {
                            var maxFactYear = (await db.Fact_KpiMeasurements
                                .Join(db.Dim_Time, f => f.DimTimeId, t => t.Id, (_, t) => t.Year)
                                .Select(y => (int?)y)
                                .MaxAsync(cancellationToken)) ?? 1993;

                            var currentYear = DateTime.UtcNow.Year;
                            var startYear = Math.Clamp(maxFactYear + 1, 1994, currentYear);

                            return new SyncAllFactsCommand(startYear, currentYear);
                        }
                    }
                };

            foreach (var job in jobs)
            {
                var key = job.Key;

                try
                {
                    // Check timestamp for job
                    var settings = await db.SystemSettings.FindAsync([key], cancellationToken: cancellationToken)
                        ?? new SystemSetting { Id = key, LastSync = DateTimeOffset.MinValue };
                    var timeSinceLastRun = DateTimeOffset.UtcNow - settings.LastSync;

                    var shouldRun = timeSinceLastRun > TimeSpan.FromDays(7);

                    if (key == "Sync.Fact")
                    {
                        var maxFactYear = (await db.Fact_KpiMeasurements
                            .Join(db.Dim_Time, f => f.DimTimeId, t => t.Id, (_, t) => t.Year)
                            .Select(y => (int?)y)
                            .MaxAsync(cancellationToken)) ?? 1993;

                        shouldRun = shouldRun || maxFactYear < DateTime.UtcNow.Year;
                    }

                    // Check if there has been more than a week since last run for this job
                    if (shouldRun)
                    {
                        var command = await job.Value();
                        _logger.LogInformation("Starting sync for job: {JobKey}", key);

                        // Fire job and wait for it complete before we start another
                        await bus.InvokeAsync(command);

                        var shouldUpdateSchedule = true;

                        if (key == "Sync.Fact")
                        {
                            var maxFactYearAfterRun = (await db.Fact_KpiMeasurements
                                .Join(db.Dim_Time, f => f.DimTimeId, t => t.Id, (_, t) => t.Year)
                                .Select(y => (int?)y)
                                .MaxAsync(cancellationToken)) ?? 1993;

                            var currentYear = DateTime.UtcNow.Year;
                            shouldUpdateSchedule = maxFactYearAfterRun >= currentYear;

                            if (!shouldUpdateSchedule)
                            {
                                _logger.LogInformation(
                                    "Fact sync still catching up. Max imported year is {MaxYear}, current year is {CurrentYear}. LastSync will not be updated yet.",
                                    maxFactYearAfterRun,
                                    currentYear);
                            }
                        }

                        if (shouldUpdateSchedule)
                        {
                            settings.LastSync = DateTimeOffset.UtcNow;

                            if (db.Entry(settings).State == EntityState.Detached)
                            {
                                db.Add(settings);
                            }

                            await db.SaveChangesAsync(cancellationToken);
                            _logger.LogInformation("Finished job {JobKey} and updated schedule", key);
                        }
                        else
                        {
                            _logger.LogInformation("Finished job {JobKey} without updating schedule", key);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to run {JobKey}", job.Key);
                }
            }
        }
    }
}
