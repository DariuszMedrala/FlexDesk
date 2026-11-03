using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FlexDesk.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<FlexDeskDbContext>
{
    private const string FallbackConnectionString =
        "Host=localhost;Port=5432;Database=flexdesk;Username=flexdesk;Password=flexdesk_dev_pw";

    public FlexDeskDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__Default") ?? FallbackConnectionString;

        var options = new DbContextOptionsBuilder<FlexDeskDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new FlexDeskDbContext(options);
    }
}
