using Reqnroll;

namespace Banking.Specs.Support;

[Binding]
public sealed class DbHooks
{
    private readonly BankingApiFactory _factory;

    public DbHooks(BankingApiFactory factory)
    {
        _factory = factory;
    }

    [BeforeScenario(Order = 0)]
    public async Task ResetDb()
    {
        await _factory.ResetDbAsync();
    }
}