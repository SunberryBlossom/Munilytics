using Microsoft.EntityFrameworkCore.Design;

namespace Munilytics.Server.Infrastructure.Persistence
{
    public sealed class MunilyticsDbContextFactory : IDesignTimeDbContextFactory<MunilyticsDbContext>
    {
        public MunilyticsDbContext CreateDbContext(string[] args)
        {
            throw new NotImplementedException();
        }
    }
}
