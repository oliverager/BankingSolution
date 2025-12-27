using Banking.Core.Entities;
using Banking.Core.Interfaces;
using Banking.Core.Interfaces.Repositories;
using Banking.Core.Services;
using Banking.Core.Transfers;
using Moq;

namespace Banking.UnitTests;

public class TransferDecisionTableTests
{
    private TransferService CreateService()
    {
        var accounts = new Mock<IRepository<Account>>();
        var transactions = new Mock<IRepository<Transaction>>();
        return new TransferService(accounts.Object, transactions.Object);
    }

    [Theory]
    [InlineData(CustomerType.Standard, 500, true, null)]
    [InlineData(CustomerType.Standard, 5_000, false, "LimitExceeded")]
    [InlineData(CustomerType.Premium, 5_000, true, null)]
    [InlineData(CustomerType.Premium, 20_000, false, "RequiresManualApproval")]
    public void EvaluateLimit_FollowsDecisionTable(CustomerType type, decimal amount, bool allowed, string? reason)
    {
        var service = CreateService();

        var decision = service.EvaluateLimit(type, amount);

        Assert.Equal(allowed, decision.Allowed);
        Assert.Equal(reason, decision.Reason);
    }
}