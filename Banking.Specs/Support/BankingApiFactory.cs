using System.Data.Common;
using Banking.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Banking.Specs.Support;

public class BankingApiFactory : WebApplicationFactory<Program>
{
    private DbConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove DbContextOptions registration
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<BankingDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            // Keep ONE in-memory database alive
            _connection = new SqliteConnection("Data Source=:memory:;Cache=Shared");
            _connection.Open();

            services.AddDbContext<BankingDbContext>(o => o.UseSqlite(_connection));
        });
    }

    public async Task ResetDbAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BankingDbContext>();

        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }


    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection?.Dispose();
        _connection = null;
    }
}