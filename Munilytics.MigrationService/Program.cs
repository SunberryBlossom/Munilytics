using Munilytics.Server.Infrastructure.Persistence;

namespace Munilytics.MigrationService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.AddNpgsqlDbContext<MunilyticsDbContext>("MunilyticsDb");

            builder.Services.AddHostedService<Worker>();
            var host = builder.Build();
            host.Run();
        }
    }
}
