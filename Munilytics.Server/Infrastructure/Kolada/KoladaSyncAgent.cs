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
                        if (key == "Sync.Fact")
                        {
                            var hasPendingFactSyncWork = await HasPendingFactSyncWork(db, cancellationToken);
                            if (hasPendingFactSyncWork)
                            {
                                _logger.LogInformation(
                                    "Skipping {JobKey} enqueue because pending fact sync envelopes already exist in Wolverine.",
                                    key);
                                continue;
                            }
                        }

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

        private static async Task<bool> HasPendingFactSyncWork(MunilyticsDbContext db, CancellationToken cancellationToken)
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
