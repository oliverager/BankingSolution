using System.Data.Common;
using Banking.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Banking.ApiTests;

public class BankingApiFactory : WebApplicationFactory<Program>
{
    private DbConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveDbContext<BankingDbContext>();

            // ONE shared connection for the whole factory lifetime
            _connection = new SqliteConnection("Data Source=:memory:;Cache=Shared");
            _connection.Open();

            services.AddDbContext<BankingDbContext>(opt => opt.UseSqlite(_connection));
        });
    }

    // IMPORTANT: create schema using the REAL host container
    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        // Create schema on the real host provider (same DbContext + same connection)
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BankingDbContext>();
        db.Database.EnsureCreated();

        return host;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection?.Dispose();
        _connection = null;
    }
}