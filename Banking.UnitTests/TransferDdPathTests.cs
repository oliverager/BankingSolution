using Banking.Core.Entities;
using Banking.Core.Interfaces;

using Banking.Core.Transfers;

using Moq;

namespace Banking.UnitTests;

public class TransferDdPathTests
{
    private (TransferService service, Mock<IRepository<Account>> accounts, Mock<IRepository<Transaction>> transactions) Create()
    {
        var accountRepo = new Mock<IRepository<Account>>();
        var txRepo = new Mock<IRepository<Transaction>>();
        return (new TransferService(accountRepo.Object, txRepo.Object), accountRepo, txRepo);
    }

    [Fact]
    public async Task Transfer_ReturnsAccountNotFound_WhenAnyAccountMissing()
    {
        var (service, accounts, _) = Create();

        accounts.Setup(a => a.GetByIdAsync(It.IsAny<Guid>(), default))
                .ReturnsAsync((Account?)null);

        var result = await service.TransferAsync(Guid.NewGuid(), Guid.NewGuid(), 100);

        Assert.False(result.Success);
        Assert.Equal("AccountNotFound", result.Reason);
    }

    [Fact]
    public async Task Transfer_ReturnsAccountInactive_WhenAnyInactive()
    {
        var (service, accounts, _) = Create();

        var customer = new Customer { Name = "Test", Type = CustomerType.Standard };
        var from = new Account { Id = Guid.NewGuid(), IsActive = false, Balance = 1_000, Customer = customer };
        var to = new Account { Id = Guid.NewGuid(), IsActive = true, Balance = 0, Customer = customer };

        accounts.Setup(a => a.GetByIdAsync(from.Id, default)).ReturnsAsync(from);
        accounts.Setup(a => a.GetByIdAsync(to.Id, default)).ReturnsAsync(to);

        var result = await service.TransferAsync(from.Id, to.Id, 100);

        Assert.False(result.Success);
        Assert.Equal("AccountInactive", result.Reason);
    }

    [Fact]
    public async Task Transfer_ReturnsInvalidAmount_WhenAmountNonPositive()
    {
        var (service, accounts, _) = Create();

        var customer = new Customer { Name = "Test", Type = CustomerType.Standard };
        var from = new Account { Id = Guid.NewGuid(), IsActive = true, Balance = 1_000, Customer = customer };
        var to = new Account { Id = Guid.NewGuid(), IsActive = true, Balance = 0, Customer = customer };

        accounts.Setup(a => a.GetByIdAsync(from.Id, default)).ReturnsAsync(from);
        accounts.Setup(a => a.GetByIdAsync(to.Id, default)).ReturnsAsync(to);

        var result = await service.TransferAsync(from.Id, to.Id, 0);

        Assert.False(result.Success);
        Assert.Equal("InvalidAmount", result.Reason);
    }

    [Fact]
    public async Task Transfer_ReturnsInsufficientBalance_WhenBalanceTooLow()
    {
        var (service, accounts, _) = Create();

        var customer = new Customer { Name = "Test", Type = CustomerType.Standard };
        var from = new Account { Id = Guid.NewGuid(), IsActive = true, Balance = 50, Customer = customer };
        var to = new Account { Id = Guid.NewGuid(), IsActive = true, Balance = 0, Customer = customer };

        accounts.Setup(a => a.GetByIdAsync(from.Id, default)).ReturnsAsync(from);
        accounts.Setup(a => a.GetByIdAsync(to.Id, default)).ReturnsAsync(to);

        var result = await service.TransferAsync(from.Id, to.Id, 100);

        Assert.False(result.Success);
        Assert.Equal("InsufficientBalance", result.Reason);
    }

    [Fact]
    public async Task Transfer_ReturnsLimitExceeded_WhenDecisionTableDenies()
    {
        var (service, accounts, _) = Create();

        var customer = new Customer { Name = "Test", Type = CustomerType.Standard };
        var from = new Account { Id = Guid.NewGuid(), IsActive = true, Balance = 10_000, Customer = customer };
        var to = new Account { Id = Guid.NewGuid(), IsActive = true, Balance = 0, Customer = customer };

        accounts.Setup(a => a.GetByIdAsync(from.Id, default)).ReturnsAsync(from);
        accounts.Setup(a => a.GetByIdAsync(to.Id, default)).ReturnsAsync(to);

        var result = await service.TransferAsync(from.Id, to.Id, 5_000);

        Assert.False(result.Success);
        Assert.Equal("LimitExceeded", result.Reason);
    }

    [Fact]
    public async Task Transfer_Succeeds_WhenAllChecksPass()
    {
        var (service, accounts, transactions) = Create();

        var customer = new Customer { Name = "Test", Type = CustomerType.Premium };
        var from = new Account { Id = Guid.NewGuid(), IsActive = true, Balance = 2_000, Customer = customer };
        var to = new Account { Id = Guid.NewGuid(), IsActive = true, Balance = 500, Customer = customer };

        accounts.Setup(a => a.GetByIdAsync(from.Id, default)).ReturnsAsync(from);
        accounts.Setup(a => a.GetByIdAsync(to.Id, default)).ReturnsAsync(to);

        var result = await service.TransferAsync(from.Id, to.Id, 1_000);

        Assert.True(result.Success);
        Assert.Null(result.Reason);
        Assert.Equal(1_000, from.Balance);
        Assert.Equal(1_500, to.Balance);

        transactions.Verify(t => t.AddAsync(It.IsAny<Transaction>(), default), Times.Once);
    }
}
