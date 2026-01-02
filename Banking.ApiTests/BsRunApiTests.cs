using System.Net.Http.Json;
using Banking.Core.Entities;
using Banking.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Banking.ApiTests;

public class BsRunApiTests : IClassFixture<BankingApiFactory>
{
    private readonly BankingApiFactory _factory;

    public BsRunApiTests(BankingApiFactory factory)
    {
        _factory = factory;
    }

    private async Task SeedAsync(Action<BankingDbContext> seed)
    {
        _factory.CreateClient(); // force host creation + EnsureCreated

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BankingDbContext>();

        seed(db);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task NotifyUpcoming_ReturnsNotifiedCount()
    {
        var client = _factory.CreateClient();
        var now = DateTime.UtcNow;

        await SeedAsync(db =>
        {
            var mandateId = Guid.NewGuid();

            db.Mandates.Add(new Mandate
            {
                Id = mandateId,
                Status = MandateStatus.Active,
                PayerAccountId = Guid.NewGuid(),
                SettlementAccountId = Guid.NewGuid(),
                DebtorCustomerId = Guid.NewGuid(),
                CreatedUtc = DateTime.UtcNow.AddDays(-2),
                ActivatedUtc = DateTime.UtcNow.AddDays(-1)
            });

            db.Collections.Add(new Collection
            {
                Id = Guid.NewGuid(),
                MandateId = mandateId,
                Status = CollectionStatus.Created,
                DueDateUtc = now.Date.AddDays(7),
                Amount = 199m,
                CreatedUtc = DateTime.UtcNow
            });
        });

        var resp = await client.PostAsync("/bs/notify?daysAhead=7", content: null);
        resp.EnsureSuccessStatusCode();

        var payload = await resp.Content.ReadFromJsonAsync<Dictionary<string, int>>();
        Assert.NotNull(payload);
        Assert.True(payload!.ContainsKey("notified"));
        Assert.Equal(1, payload["notified"]);
    }

    [Fact]
    public async Task CollectDue_ReturnsCollectedCount()
    {
        var client = _factory.CreateClient();
        var now = DateTime.UtcNow;

        var payerId = Guid.NewGuid();
        var settlementId = Guid.NewGuid();
        var mandateId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            // Accounts are needed for a real transfer if your transfer service loads accounts from DB.
            // Seed minimal accounts:
            var customerId = Guid.NewGuid();
            db.Customers.Add(new Customer { Id = customerId, Name = "Seed", Type = CustomerType.Standard });

            db.Accounts.Add(new Account
            {
                Id = payerId,
                CustomerId = customerId,
                Iban = "DKTESTPAYER000000000000",
                Balance = 10_000m,
                IsActive = true
            });

            db.Accounts.Add(new Account
            {
                Id = settlementId,
                CustomerId = customerId,
                Iban = "DKTESTSETTLE0000000000",
                Balance = 0m,
                IsActive = true
            });

            db.Mandates.Add(new Mandate
            {
                Id = mandateId,
                Status = MandateStatus.Active,
                PayerAccountId = payerId,
                SettlementAccountId = settlementId,
                DebtorCustomerId = customerId,
                CreatedUtc = DateTime.UtcNow.AddDays(-2),
                ActivatedUtc = DateTime.UtcNow.AddDays(-1)
            });

            db.Collections.Add(new Collection
            {
                Id = Guid.NewGuid(),
                MandateId = mandateId,
                Status = CollectionStatus.Approved,
                DueDateUtc = now.Date,
                Amount = 250m,
                CreatedUtc = DateTime.UtcNow.AddDays(-1)
            });
        });

        var resp = await client.PostAsync("/bs/collect", content: null);
        resp.EnsureSuccessStatusCode();

        var payload = await resp.Content.ReadFromJsonAsync<Dictionary<string, int>>();
        Assert.NotNull(payload);
        Assert.True(payload!.ContainsKey("collected"));
        Assert.Equal(1, payload["collected"]);
    }
}