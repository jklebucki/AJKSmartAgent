using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Praxiara.Infrastructure.Persistence;

public sealed class PraxiaraDbContextFactory : IDesignTimeDbContextFactory<PraxiaraDbContext>
{
    public PraxiaraDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("PRAXIARA_DESIGNTIME_CONNECTION_STRING") ??
            "Host=localhost;Port=5432;Database=praxiara;Username=praxiara_migrator";
        var options = new DbContextOptionsBuilder<PraxiaraDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new PraxiaraDbContext(options);
    }
}