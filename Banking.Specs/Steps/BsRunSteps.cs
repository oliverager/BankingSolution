using System.Net.Http.Json;
using Banking.Core.Entities;
using Banking.Infrastructure;
using Banking.Specs.Support;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;

namespace Banking.Specs.Steps;

[Binding]
public class BsRunSteps
{
    private readonly BankingApiFactory _factory;
    private readonly HttpClient _client;
    private readonly ScenarioContext _ctx;

    private HttpResponseMessage? _lastResponse;
    private Guid _mandateId;
    private Guid _payerAccountId;
    private Guid _settlementAccountId;
    private Guid _lastCollectionId;

    public BsRunSteps(BankingApiFactory factory, ScenarioContext ctx)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _ctx = ctx;
    }

    [Given("the system is running")]
    public void GivenSystemRunning()
    {
        // no-op (factory spins the API)
    }

    [Given("an active mandate exists for BS")]
    public async Task GivenAnActiveMandateExistsForBs()
    {
        await SeedMandateAsync(active: true);
    }

    [Given("a cancelled mandate exists for BS")]
    public async Task GivenACancelledMandateExistsForBs()
    {
        await SeedMandateAsync(active: false);
    }

    private async Task SeedMandateAsync(bool active)
    {
        _mandateId = Guid.NewGuid();
        _payerAccountId = Guid.NewGuid();
        _settlementAccountId = Guid.NewGuid();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BankingDbContext>();

        var customerId = Guid.NewGuid();

        db.Customers.Add(new Customer
        {
            Id = customerId,
            Name = "BS BDD",
            Type = CustomerType.Standard
        });

        db.Accounts.Add(new Account
        {
            Id = _payerAccountId,
            CustomerId = customerId,
            Iban = "DKBSBDD0000000000000001",
            Balance = 10_000m,
            IsActive = true
        });

        db.Accounts.Add(new Account
        {
            Id = _settlementAccountId,
            CustomerId = customerId,
            Iban = "DKBSBDD0000000000000999",
            Balance = 0m,
            IsActive = true
        });

        db.Mandates.Add(new Mandate
        {
            Id = _mandateId,
            DebtorCustomerId = customerId,
            PayerAccountId = _payerAccountId,
            SettlementAccountId = _settlementAccountId,
            Status = active ? MandateStatus.Active : MandateStatus.Cancelled,
            CreatedUtc = DateTime.UtcNow.AddDays(-2),
            ActivatedUtc = active ? DateTime.UtcNow.AddDays(-1) : null,
            CancelledUtc = active ? null : DateTime.UtcNow.AddDays(-1)
        });

        await db.SaveChangesAsync();
    }

    [Given(@"a collection is created with due date (.*) days from today and amount (.*)")]
    public async Task GivenACollectionIsCreatedWithDueDateDaysFromTodayAndAmount(int daysFromToday, decimal amount)
    {
        _lastCollectionId = Guid.NewGuid();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BankingDbContext>();

        db.Collections.Add(new Collection
        {
            Id = _lastCollectionId,
            MandateId = _mandateId,
            Status = CollectionStatus.Created,
            DueDateUtc = DateTime.UtcNow.Date.AddDays(daysFromToday),
            Amount = amount,
            Text = "BDD upcoming",
            CreatedUtc = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
    }

    [Given(@"a collection is approved with due date today and amount (.*)")]
    public async Task GivenACollectionIsApprovedWithDueDateTodayAndAmount(decimal amount)
    {
        _lastCollectionId = Guid.NewGuid();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BankingDbContext>();

        db.Collections.Add(new Collection
        {
            Id = _lastCollectionId,
            MandateId = _mandateId,
            Status = CollectionStatus.Approved,
            DueDateUtc = DateTime.UtcNow.Date,
            Amount = amount,
            Text = "BDD due",
            CreatedUtc = DateTime.UtcNow.AddDays(-1),
            NotifiedUtc = DateTime.UtcNow.AddDays(-1),
            DecisionUtc = DateTime.UtcNow.AddDays(-1)
        });

        await db.SaveChangesAsync();
    }

    [When(@"I run BS notify upcoming with daysAhead (.*)")]
    public async Task WhenIRunBsNotifyUpcomingWithDaysAhead(int daysAhead)
    {
        _lastResponse = await _client.PostAsync($"/bs/notify?daysAhead={daysAhead}", content: null);
    }

    [When("I run BS collect due")]
    public async Task WhenIRunBsCollectDue()
    {
        _lastResponse = await _client.PostAsync("/bs/collect", content: null);
    }

    [Then(@"the notify result should report (.*) notified")]
    public async Task ThenNotifyResultShouldReportNotified(int expected)
    {
        _lastResponse.Should().NotBeNull();
        _lastResponse!.EnsureSuccessStatusCode();

        var payload = await _lastResponse.Content.ReadFromJsonAsync<Dictionary<string, int>>();
        payload.Should().NotBeNull();
        payload!.Should().ContainKey("notified");
        payload["notified"].Should().Be(expected);
    }

    [Then(@"the collect result should report (.*) collected")]
    public async Task ThenCollectResultShouldReportCollected(int expected)
    {
        _lastResponse.Should().NotBeNull();
        _lastResponse!.EnsureSuccessStatusCode();

        var payload = await _lastResponse.Content.ReadFromJsonAsync<Dictionary<string, int>>();
        payload.Should().NotBeNull();
        payload!.Should().ContainKey("collected");
        payload["collected"].Should().Be(expected);
    }

    [Then(@"the last collection status should be Failed with reason ""(.*)""")]
    public async Task ThenLastCollectionStatusShouldBeFailedWithReason(string reason)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BankingDbContext>();

        var c = await db.Collections.FindAsync(_lastCollectionId);
        c.Should().NotBeNull();
        c!.Status.Should().Be(CollectionStatus.Failed);
        c.FailureReason.Should().Be(reason);
    }
}
