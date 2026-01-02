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

    public AccountTransferSteps(BankingApiFactory factory, ScenarioContext scenario)
    {
        _client = factory.CreateClient();
        _scenario = scenario;
    }

    private Dictionary<string, Guid> Accounts =>
        _scenario.ContainsKey("accounts")
            ? (Dictionary<string, Guid>)_scenario["accounts"]
            : (Dictionary<string, Guid>)(_scenario["accounts"] = new Dictionary<string, Guid>());

    private Guid CustomerId
    {
        get => (Guid)_scenario["customerId"];
        set => _scenario["customerId"] = value;
    }

    [Given(@"a Standard customer with an account ""(.*)"" having balance (.*)")]
    public async Task GivenAStandardCustomerWithAccount(string alias, decimal balance)
    {
        var customerResponse = await _client.PostAsJsonAsync("/customers", new CreateCustomerRequest
        {
            Name = "BDD Customer",
            Type = CustomerType.Standard
        });

        customerResponse.EnsureSuccessStatusCode();
        var customer = await customerResponse.Content.ReadFromJsonAsync<Customer>();
        customer.Should().NotBeNull();
        CustomerId = customer!.Id;

        var accountResponse = await _client.PostAsJsonAsync("/accounts", new CreateAccountRequest
        {
            CustomerId = CustomerId,
            Iban = $"DKBDD{alias}000000000000",
            InitialBalance = balance
        });

        accountResponse.EnsureSuccessStatusCode();
        var account = await accountResponse.Content.ReadFromJsonAsync<Account>();
        account.Should().NotBeNull();

        Accounts[alias] = account!.Id;
    }

    [Given(@"an active account ""(.*)"" for the same customer with balance (.*)")]
    public async Task GivenAnotherActiveAccount(string alias, decimal balance)
    {
        var accountResponse = await _client.PostAsJsonAsync("/accounts", new CreateAccountRequest
        {
            CustomerId = CustomerId,
            Iban = $"DKBDD{alias}000000000000",
            InitialBalance = balance
        });

        accountResponse.EnsureSuccessStatusCode();
        var account = await accountResponse.Content.ReadFromJsonAsync<Account>();
        account.Should().NotBeNull();

        Accounts[alias] = account!.Id;
    }

    [When(@"the customer transfers (.*) from ""(.*)"" to ""(.*)""")]
    public async Task WhenCustomerTransfers(decimal amount, string fromAlias, string toAlias)
    {
        var response = await _client.PostAsJsonAsync("/transfers", new TransferRequest
        {
            FromAccountId = Accounts[fromAlias],
            ToAccountId = Accounts[toAlias],
            Amount = amount
        });

        _lastResponse = response;
    }

    [Then(@"the transfer should be accepted")]
    public void ThenTransferAccepted()
    {
        _lastResponse.Should().NotBeNull();
        _lastResponse!.IsSuccessStatusCode.Should().BeTrue();
    }

    [Then(@"the transfer should be rejected with reason ""(.*)""")]
    public async Task ThenTransferRejectedWithReason(string reason)
    {
        _lastResponse.Should().NotBeNull();
        _lastResponse!.IsSuccessStatusCode.Should().BeFalse();

        var body = await _lastResponse.Content.ReadAsStringAsync();
        body.Should().Contain(reason);
    }

    [Then(@"the balance of ""(.*)"" should be (.*)")]
    public async Task ThenBalanceShouldBe(string alias, decimal expected)
    {
        var accountId = Accounts[alias];

        var resp = await _client.GetAsync($"/accounts/{accountId}");
        resp.EnsureSuccessStatusCode();

        var account = await resp.Content.ReadFromJsonAsync<Account>();
        account.Should().NotBeNull();
        account!.Balance.Should().Be(expected);
    }
    
    [Then("the transfer should succeed")]
    public void ThenTheTransferShouldSucceed()
    {
        ThenTransferAccepted();
    }

}