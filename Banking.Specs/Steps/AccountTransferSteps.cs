using System.Net.Http.Json;
using Banking.Api.Contracts;
using Banking.Core.Entities;
using Banking.Specs.Support;
using FluentAssertions;
using Reqnroll;

namespace Banking.Specs.Steps;

[Binding]
public class AccountTransferSteps
{
    private readonly HttpClient _client;
    private readonly ScenarioContext _scenario;
    private HttpResponseMessage? _lastResponse;

    private readonly Dictionary<string, Guid> _accountAliases = new();

    public AccountTransferSteps(ScenarioContext scenarioContext)
    {
        var factory = new BankingApiFactory();
        _client = factory.CreateClient();
        _scenario = scenarioContext;
    }

    [Given(@"a Standard customer with an account ""(.*)"" having balance (.*)")]
    public async Task GivenCustomerWithAccount(string alias, decimal balance)
    {
        var customerResponse = await _client.PostAsJsonAsync("/customers", new CreateCustomerRequest
        {
            Name = "BDD Customer",
            Type = CustomerType.Standard
        });

        customerResponse.EnsureSuccessStatusCode();
        var customer = await customerResponse.Content.ReadFromJsonAsync<Customer>();
        customer.Should().NotBeNull();

        var accountResponse = await _client.PostAsJsonAsync("/accounts", new CreateAccountRequest
        {
            CustomerId = customer!.Id,
            Iban = Guid.NewGuid().ToString("N"),
            InitialBalance = balance
        });

        accountResponse.EnsureSuccessStatusCode();
        var account = await accountResponse.Content.ReadFromJsonAsync<Account>();
        account.Should().NotBeNull();

        _accountAliases[alias] = account!.Id;
    }

    [Given(@"an active account ""(.*)"" for the same customer with balance (.*)")]
    public async Task GivenAnotherAccount(string alias, decimal balance)
    {
        // re-use last created customer
        var customersResponse = await _client.GetAsync("/customers");
        // to keep it simple, assume previous step created a customer and we
        // reuse that ID – for an exam you can simplify or store it explicitly.
        // Here we'll just create a new Standard customer again for clarity

        var customerResponse = await _client.PostAsJsonAsync("/customers", new CreateCustomerRequest
        {
            Name = "BDD Customer 2",
            Type = CustomerType.Standard
        });

        customerResponse.EnsureSuccessStatusCode();
        var customer = await customerResponse.Content.ReadFromJsonAsync<Customer>();

        var accountResponse = await _client.PostAsJsonAsync("/accounts", new CreateAccountRequest
        {
            CustomerId = customer!.Id,
            Iban = Guid.NewGuid().ToString("N"),
            InitialBalance = balance
        });

        accountResponse.EnsureSuccessStatusCode();
        var account = await accountResponse.Content.ReadFromJsonAsync<Account>();

        _accountAliases[alias] = account!.Id;
    }

    [When(@"the customer transfers (.*) from ""(.*)"" to ""(.*)""")]
    public async Task WhenTheCustomerTransfers(decimal amount, string fromAlias, string toAlias)
    {
        var fromId = _accountAliases[fromAlias];
        var toId = _accountAliases[toAlias];

        var response = await _client.PostAsJsonAsync("/transfers", new TransferRequest
        {
            FromAccountId = fromId,
            ToAccountId = toId,
            Amount = amount
        });

        _lastResponse = response;
    }

    [Then(@"the transfer should succeed")]
    public async Task ThenTheTransferShouldSucceed()
    {
        _lastResponse.Should().NotBeNull();
        _lastResponse!.IsSuccessStatusCode.Should().BeTrue();
        var content = await _lastResponse.Content.ReadFromJsonAsync<dynamic>();
        ((bool)content!.success).Should().BeTrue();
    }

    [Then(@"the balance of ""(.*)"" should be (.*)")]
    public async Task ThenTheBalanceShouldBe(string alias, decimal expected)
    {
        var accountId = _accountAliases[alias];
        var response = await _client.GetAsync($"/accounts/{accountId}");
        response.EnsureSuccessStatusCode();

        var account = await response.Content.ReadFromJsonAsync<Account>();
        account.Should().NotBeNull();
        account!.Balance.Should().Be(expected);
    }

    [Then(@"the transfer should be rejected with reason ""(.*)""")]
    public async Task ThenTheTransferShouldBeRejected(string reason)
    {
        _lastResponse.Should().NotBeNull();
        _lastResponse!.IsSuccessStatusCode.Should().BeFalse();

        var error = await _lastResponse.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        error.Should().NotBeNull();
        error!["error"].Should().Be(reason);
    }
}
