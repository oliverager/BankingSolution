using Banking.Core.Entities;
using Banking.Core.Interfaces.Repositories;
using Banking.Core.Services;
using Moq;

namespace Banking.UnitTests;

public class TransferDdPathTests
{
    private static (TransferService service, Mock<IRepository<Account>> accounts, Mock<IRepository<Transaction>> txs)
        Create()
    {
        var accountRepo = new Mock<IRepository<Account>>();
        var txRepo = new Mock<IRepository<Transaction>>();

        // Default: SaveChanges works
        accountRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        txRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var service = new TransferService(accountRepo.Object, txRepo.Object);
        return (service, accountRepo, txRepo);
    }

    [Fact]
    public async Task Transfer_ReturnsAccountNotFound_WhenFromMissing()
    {
        var (service, accounts, _) = Create();

        accounts.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        var result = await service.TransferAsync(Guid.NewGuid(), Guid.NewGuid(), 100m);

        Assert.False(result.Success);
        Assert.Equal("AccountNotFound", result.Reason);
    }

    [Fact]
    public async Task Transfer_ReturnsAccountNotFound_WhenToMissing()
    {
        var (service, accounts, _) = Create();

        var customer = new Customer { Id = Guid.NewGuid(), Name = "T", Type = CustomerType.Standard };
        var from = new Account { Id = Guid.NewGuid(), IsActive = true, Balance = 1000m, Customer = customer };
        Account? to = null;

        accounts.Setup(r => r.GetByIdAsync(from.Id, It.IsAny<CancellationToken>())).ReturnsAsync(from);
        accounts.Setup(r => r.GetByIdAsync(It.Is<Guid>(id => id != from.Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(to);

        var result = await service.TransferAsync(from.Id, Guid.NewGuid(), 100m);

        Assert.False(result.Success);
        Assert.Equal("AccountNotFound", result.Reason);
    }

    [Fact]
    public async Task Transfer_ReturnsInvalidAmount_WhenAmountNonPositive()
    {
        var (service, accounts, _) = Create();

        var customer = new Customer { Id = Guid.NewGuid(), Name = "T", Type = CustomerType.Standard };
        var from = new Account { Id = Guid.NewGuid(), IsActive = true, Balance = 1000m, Customer = customer };
        var to = new Account { Id = Guid.NewGuid(), IsActive = true, Balance = 0m, Customer = customer };

        accounts.Setup(r => r.GetByIdAsync(from.Id, It.IsAny<CancellationToken>())).ReturnsAsync(from);
        accounts.Setup(r => r.GetByIdAsync(to.Id, It.IsAny<CancellationToken>())).ReturnsAsync(to);

        var result = await service.TransferAsync(from.Id, to.Id, 0m);

        Assert.False(result.Success);
        Assert.Equal("InvalidAmount", result.Reason);
    }

    [Fact]
    public async Task Transfer_ReturnsAccountInactive_WhenEitherInactive()
    {
        var (service, accounts, _) = Create();

        var customer = new Customer { Id = Guid.NewGuid(), Name = "T", Type = CustomerType.Standard };
        var from = new Account { Id = Guid.NewGuid(), IsActive = false, Balance = 1000m, Customer = customer };
        var to = new Account { Id = Guid.NewGuid(), IsActive = true, Balance = 0m, Customer = customer };

        accounts.Setup(r => r.GetByIdAsync(from.Id, It.IsAny<CancellationToken>())).ReturnsAsync(from);
        accounts.Setup(r => r.GetByIdAsync(to.Id, It.IsAny<CancellationToken>())).ReturnsAsync(to);

        var result = await service.TransferAsync(from.Id, to.Id, 100m);

        Assert.False(result.Success);
        Assert.Equal("AccountInactive", result.Reason);
    }

    [Fact]
    public async Task Transfer_ReturnsInsufficientBalance_WhenBalanceTooLow()
    {
        var (service, accounts, _) = Create();

        var customer = new Customer { Id = Guid.NewGuid(), Name = "T", Type = CustomerType.Standard };
        var from = new Account { Id = Guid.NewGuid(), IsActive = true, Balance = 50m, Customer = customer };
        var to = new Account { Id = Guid.NewGuid(), IsActive = true, Balance = 0m, Customer = customer };

        accounts.Setup(r => r.GetByIdAsync(from.Id, It.IsAny<CancellationToken>())).ReturnsAsync(from);
        accounts.Setup(r => r.GetByIdAsync(to.Id, It.IsAny<CancellationToken>())).ReturnsAsync(to);

        var result = await service.TransferAsync(from.Id, to.Id, 100m);

        Assert.False(result.Success);
        Assert.Equal("InsufficientBalance", result.Reason);
    }

    [Fact]
    public async Task Transfer_Succeeds_AndCreatesTransaction_WhenHappyPath()
    {
        var (service, accounts, txs) = Create();

        var customer = new Customer { Id = Guid.NewGuid(), Name = "T", Type = CustomerType.Standard };
        var from = new Account { Id = Guid.NewGuid(), IsActive = true, Balance = 1000m, Customer = customer };
        var to = new Account { Id = Guid.NewGuid(), IsActive = true, Balance = 200m, Customer = customer };

        accounts.Setup(r => r.GetByIdAsync(from.Id, It.IsAny<CancellationToken>())).ReturnsAsync(from);
        accounts.Setup(r => r.GetByIdAsync(to.Id, It.IsAny<CancellationToken>())).ReturnsAsync(to);

        txs.Setup(r => r.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await service.TransferAsync(from.Id, to.Id, 300m);

        Assert.True(result.Success);
        Assert.Null(result.Reason);

        Assert.Equal(700m, from.Balance);
        Assert.Equal(500m, to.Balance);

        txs.Verify(r => r.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Once);
        accounts.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }
}