using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Testcontainers.PostgreSql;

namespace FlexDesk.Tests.Integration;

public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string TestJwtSecret = "integration-tests-secret-key-0123456789-abcdefghijklmnop";

    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        await _database.StartAsync();
        Environment.SetEnvironmentVariable("ConnectionStrings__Default", _database.GetConnectionString());
        Environment.SetEnvironmentVariable("Jwt__Secret", TestJwtSecret);
    }

    public new async Task DisposeAsync()
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__Default", null);
        Environment.SetEnvironmentVariable("Jwt__Secret", null);
        await _database.DisposeAsync();
        await base.DisposeAsync();
    }
}
