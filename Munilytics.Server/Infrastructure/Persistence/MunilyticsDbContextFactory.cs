using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;

namespace Munilytics.Server.Infrastructure.Persistence
{
    public sealed class MunilyticsDbContextFactory : IDesignTimeDbContextFactory<MunilyticsDbContext>
    {
        public MunilyticsDbContext CreateDbContext(string[] args)
        {
            const string connectionString = "Host=localhost;Port=64415;Database=MunilyticsDb;Username=postgres;Password=postgres";

            var options = new DbContextOptionsBuilder<MunilyticsDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            return new MunilyticsDbContext(options);
        }
    }
}
