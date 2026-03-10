using JasperFx;
using JasperFx.Core;
using Microsoft.EntityFrameworkCore;
using Munilytics.Server.Domain.Entities;
using Munilytics.Server.Features.Admin.SyncFactData;
using Munilytics.Server.Features.Admin.SyncKpis;
using Munilytics.Server.Features.Admin.SyncMunicipalities;
using Munilytics.Server.Infrastructure.Persistence;
using System.Data;
using Wolverine;
using Wolverine.Runtime.Agents;
using YamlDotNet.Serialization;

namespace Munilytics.Server.Infrastructure.Kolada
{
    public class KoladaSyncAgent : SingularAgent
    {
        private static readonly TimeSpan JobLockStaleAfter = TimeSpan.FromHours(6);
        private static readonly TimeSpan FactLockStaleAfter = TimeSpan.FromHours(24);
        private static readonly TimeSpan FactIdleWindow = TimeSpan.FromMinutes(10);

        public sealed record DashboardKpiMapping(
            string MetricKey,
            string Category,
            string KpiCode,
            string Title,
            string SourceUnit,
            string NormalizedUnit,
            string? FallbackKpiCode = null);

        public static IReadOnlyList<DashboardKpiMapping> DashboardKpiMappings { get; } =
        [
            new("cost-total", "Cost", "N15027", "Kostnad grundskola F-9, hemkommun, kr/elev", "kr/elev", "kr/elev"),
            new("cost-administration", "Cost", "N15063", "Övriga kostnader i kommunal grundskola F-9, kr/elev", "kr/elev", "kr/elev", "N15010"),
            new("cost-teaching-materials", "Cost", "N15012", "Kostnad för lärverktyg i kommunal grundskola åk 1-9, kr/elev", "kr/elev", "kr/elev"),
            new("quality-average-grades", "Quality", "N15507", "Elever i åk 9, meritvärde, hemkommun, genomsnitt (17 ämnen)", "meritvärde", "meritvärde"),
            new("quality-high-school-eligibility", "Quality", "N15428", "Elever i åk 9 som är behöriga till yrkesprogram, hemkommun, andel (%)", "andel (%)", "andel (%)"),
            new("quality-teacher-density", "Quality", "N15100", "Elever/lärare (heltidstjänst) i kommunal grundskola F-9, antal (-2023)", "elever/lärare", "lärare per 100 elever", "N15034")
        ];

        public static decimal NormalizeDashboardValue(string metricKey, decimal rawValue)
        {
            if (string.Equals(metricKey, "quality-teacher-density", StringComparison.OrdinalIgnoreCase))
            {
                return rawValue == 0m ? 0m : 100m / rawValue;
            }

            return rawValue;
        }

        public static decimal NormalizeDashboardValueByKpiCode(string kpiCode, decimal rawValue)
        {
            if (string.IsNullOrWhiteSpace(kpiCode))
            {
                return rawValue;
            }

            var mapping = DashboardKpiMappings.FirstOrDefault(m =>
                string.Equals(m.KpiCode, kpiCode, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(m.FallbackKpiCode, kpiCode, StringComparison.OrdinalIgnoreCase));

            return mapping is null ? rawValue : NormalizeDashboardValue(mapping.MetricKey, rawValue);
        }

        private readonly ILogger<KoladaSyncAgent> _logger;
        private readonly IServiceProvider _scopeFactory;
        private Timer? _timer;

        private sealed record ScheduledJob(
            string Key,
            Func<CancellationToken, Task<object>> BuildCommand,
            Func<MunilyticsDbContext, CancellationToken, Task<bool>> IsComplete,
            Func<MunilyticsDbContext, CancellationToken, Task<bool>> IsActive,
            Func<MunilyticsDbContext, CancellationToken, Task<bool>> ShouldForceRun,
            TimeSpan LockStaleAfter);

        public KoladaSyncAgent(IServiceProvider scopeFactory, ILogger<KoladaSyncAgent> logger)
            : base("kolada-sync")
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override Task startAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Sync Scheduler");

            // Check every five minutes if sync should run or locks should be recovered
            _timer = new Timer(async _ => await CheckState(cancellationToken), null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
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

            var jobs = new[]
            {
                new ScheduledJob(
                    "Sync.Kpis",
                    _ => Task.FromResult<object>(new SyncKpiCommand()),
                    (context, ct) => Task.FromResult(true),
                    (context, ct) => Task.FromResult(false),
                    (context, ct) => Task.FromResult(false),
                    JobLockStaleAfter),
                new ScheduledJob(
                    "Sync.Municipalities",
                    _ => Task.FromResult<object>(new SyncMunicipalitiesCommand()),
                    (context, ct) => Task.FromResult(true),
                    (context, ct) => Task.FromResult(false),
                    (context, ct) => Task.FromResult(false),
                    JobLockStaleAfter),
                new ScheduledJob(
                    "Sync.Fact",
                    async ct =>
                    {
                        var maxFactYear = (await db.Fact_KpiMeasurements
                            .Join(db.Dim_Time, f => f.DimTimeId, t => t.Id, (_, t) => t.Year)
                            .Select(y => (int?)y)
                            .MaxAsync(ct)) ?? 1993;

                        var currentYear = DateTime.UtcNow.Year;
                        var startYear = Math.Clamp(maxFactYear + 1, 1994, currentYear);
                        return new SyncAllFactsCommand(startYear, currentYear);
                    },
                    (context, ct) => IsFactSyncComplete(context, ct),
                        (context, ct) => IsFactSyncActive(context, ct),
                    (context, ct) => ShouldForceRunFactSync(context, ct),
                    FactLockStaleAfter)
            };

            foreach (var job in jobs)
            {
                var key = job.Key;

                try
                {
                    var settings = await db.SystemSettings.FindAsync([key], cancellationToken: cancellationToken)
                        ?? new SystemSetting { Id = key, LastSync = DateTimeOffset.UnixEpoch, InProgress = false };

                    var timeSinceLastRun = DateTimeOffset.UtcNow - settings.LastSync;
                    var shouldRun = timeSinceLastRun > TimeSpan.FromDays(7) || await job.ShouldForceRun(db, cancellationToken);

                    if (settings.InProgress)
                    {
                        var isComplete = await job.IsComplete(db, cancellationToken);
                        if (isComplete)
                        {
                            settings.InProgress = false;
                            settings.InProgressUpdatedAt = null;
                            settings.LastSync = DateTimeOffset.UtcNow;

                            if (db.Entry(settings).State == EntityState.Detached)
                            {
                                db.Add(settings);
                            }

                            await db.SaveChangesAsync(cancellationToken);
                            shouldRun = false;
                            _logger.LogInformation("Marked {JobKey} as completed from in-progress state.", key);
                        }
                        else if (settings.InProgressUpdatedAt.HasValue)
                        {
                            var lockAge = DateTimeOffset.UtcNow - settings.InProgressUpdatedAt.Value;
                            var lockIsFresh = lockAge < job.LockStaleAfter;
                            var hasActivity = await job.IsActive(db, cancellationToken);

                            if (lockIsFresh && hasActivity)
                            {
                                settings.InProgressUpdatedAt = DateTimeOffset.UtcNow;

                                if (db.Entry(settings).State == EntityState.Detached)
                                {
                                    db.Add(settings);
                                }

                                await db.SaveChangesAsync(cancellationToken);
                                shouldRun = false;
                                _logger.LogInformation(
                                    "Skipping {JobKey} enqueue because job is already in progress (age {LockAgeMinutes} minutes).",
                                    key,
                                    Math.Round(lockAge.TotalMinutes));
                            }
                            else if (lockIsFresh && !hasActivity)
                            {
                                settings.InProgress = false;
                                settings.InProgressUpdatedAt = null;

                                if (db.Entry(settings).State == EntityState.Detached)
                                {
                                    db.Add(settings);
                                }

                                await db.SaveChangesAsync(cancellationToken);
                                shouldRun = true;
                                _logger.LogWarning(
                                    "Recovered inactive in-progress lock for {JobKey} (age {LockAgeMinutes} minutes, no activity detected).",
                                    key,
                                    Math.Round(lockAge.TotalMinutes));
                            }
                            else
                            {
                                settings.InProgress = false;
                                settings.InProgressUpdatedAt = null;

                                if (db.Entry(settings).State == EntityState.Detached)
                                {
                                    db.Add(settings);
                                }

                                await db.SaveChangesAsync(cancellationToken);
                                _logger.LogWarning(
                                    "Recovered stale in-progress lock for {JobKey} (age {LockAgeHours} hours).",
                                    key,
                                    Math.Round(lockAge.TotalHours, 1));
                            }
                        }
                        else
                        {
                            settings.InProgress = false;
                            settings.InProgressUpdatedAt = null;

                            if (db.Entry(settings).State == EntityState.Detached)
                            {
                                db.Add(settings);
                            }

                            await db.SaveChangesAsync(cancellationToken);
                            _logger.LogWarning("Recovered malformed in-progress state for {JobKey} (missing heartbeat).", key);
                        }
                    }

                    if (shouldRun)
                    {
                        settings.InProgress = true;
                        settings.InProgressUpdatedAt = DateTimeOffset.UtcNow;

                        if (db.Entry(settings).State == EntityState.Detached)
                        {
                            db.Add(settings);
                        }

                        await db.SaveChangesAsync(cancellationToken);

                        var command = await job.BuildCommand(cancellationToken);
                        _logger.LogInformation("Starting sync for job: {JobKey}", key);

                        await bus.InvokeAsync(command);

                        var completedInline = await job.IsComplete(db, cancellationToken);
                        if (completedInline)
                        {
                            settings.InProgress = false;
                            settings.InProgressUpdatedAt = null;
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
                            _logger.LogInformation("Job {JobKey} started asynchronous downstream work; keeping in-progress state.", key);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to run {JobKey}", job.Key);
                }
            }
        }

        private static async Task<bool> ShouldForceRunFactSync(MunilyticsDbContext db, CancellationToken cancellationToken)
        {
            var maxFactYear = (await db.Fact_KpiMeasurements
                .Join(db.Dim_Time, f => f.DimTimeId, t => t.Id, (_, t) => t.Year)
                .Select(y => (int?)y)
                .MaxAsync(cancellationToken)) ?? 1993;

            return maxFactYear < DateTime.UtcNow.Year;
        }

        private static async Task<bool> IsFactSyncComplete(MunilyticsDbContext db, CancellationToken cancellationToken)
        {
            var maxFactYear = (await db.Fact_KpiMeasurements
                .Join(db.Dim_Time, f => f.DimTimeId, t => t.Id, (_, t) => t.Year)
                .Select(y => (int?)y)
                .MaxAsync(cancellationToken)) ?? 1993;

            if (maxFactYear < DateTime.UtcNow.Year)
            {
                return false;
            }

            if (await HasPendingFactSyncEnvelopes(db, cancellationToken))
            {
                return false;
            }

            var latestImport = await GetLatestFactImport(db, cancellationToken);
            return latestImport.HasValue && latestImport.Value < DateTime.UtcNow - FactIdleWindow;
        }

        private static async Task<bool> IsFactSyncActive(MunilyticsDbContext db, CancellationToken cancellationToken)
        {
            return await HasPendingFactSyncEnvelopes(db, cancellationToken);
        }

        private static async Task<DateTime?> GetLatestFactImport(MunilyticsDbContext db, CancellationToken cancellationToken)
        {
            return await db.Fact_KpiMeasurements
                .Select(f => (DateTime?)f.LatestUpdate)
                .MaxAsync(cancellationToken);
        }

        private static async Task<bool> HasPendingFactSyncEnvelopes(MunilyticsDbContext db, CancellationToken cancellationToken)
        {
            const string sql = """
                SELECT COUNT(*)
                FROM wolverine.wolverine_incoming_envelopes
                WHERE message_type LIKE '%SyncFactData.SyncAllFactsCommand%'
                   OR message_type LIKE '%SyncFactData.SyncFactCommand%';
                """;

            var connection = db.Database.GetDbConnection();
            var shouldCloseConnection = connection.State != ConnectionState.Open;

            if (shouldCloseConnection)
            {
                await connection.OpenAsync(cancellationToken);
            }

            try
            {
                await using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.CommandType = CommandType.Text;

                var result = await command.ExecuteScalarAsync(cancellationToken);
                var count = result is null || result is DBNull ? 0 : Convert.ToInt64(result);
                return count > 0;
            }
            finally
            {
                if (shouldCloseConnection)
                {
                    await connection.CloseAsync();
                }
            }
        }
    }
}
