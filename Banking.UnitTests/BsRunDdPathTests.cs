using Banking.Core.Entities;
using Banking.Core.Interfaces.Repositories;
using Banking.Core.Interfaces.Services;
using Banking.Core.Services;
using Banking.Core.Transfers;
using Moq;

namespace Banking.UnitTests;

public class BsRunDdPathTests
{
    private static (
        BsRunService svc,
        Mock<ICollectionRepository> colRepo,
        Mock<IMandateRepository> manRepo,
        Mock<ITransferService> transfer
    ) Create()
    {
        var colRepo = new Mock<ICollectionRepository>();
        var manRepo = new Mock<IMandateRepository>();
        var transfer = new Mock<ITransferService>();

        colRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);

        var svc = new BsRunService(colRepo.Object, manRepo.Object, transfer.Object);
        return (svc, colRepo, manRepo, transfer);
    }

    [Fact]
    public async Task NotifyUpcoming_MarksCreatedCollectionsAsNotified()
    {
        var (svc, colRepo, _, _) = Create();

        var now = DateTime.UtcNow;
        var c1 = new Collection { Id = Guid.NewGuid(), Status = CollectionStatus.Created, DueDateUtc = now.Date.AddDays(3) };
        var c2 = new Collection { Id = Guid.NewGuid(), Status = CollectionStatus.Created, DueDateUtc = now.Date.AddDays(7) };

        colRepo.Setup(r => r.GetUpcomingForNotificationAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync([c1, c2]);

        var count = await svc.NotifyUpcomingAsync(now, daysAhead: 7);

        Assert.Equal(2, count);
        Assert.Equal(CollectionStatus.Notified, c1.Status);
        Assert.Equal(CollectionStatus.Notified, c2.Status);
        Assert.NotNull(c1.NotifiedUtc);
        Assert.NotNull(c2.NotifiedUtc);

        colRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CollectDue_Fails_WhenMandateMissing()
    {
        var (svc, colRepo, manRepo, transfer) = Create();

        var now = DateTime.UtcNow;
        var col = new Collection
        {
            Id = Guid.NewGuid(),
            MandateId = Guid.NewGuid(),
            Status = CollectionStatus.Approved,
            DueDateUtc = now.Date,
            Amount = 100m
        };

        colRepo.Setup(r => r.GetDueForCollectionAsync(now, It.IsAny<CancellationToken>()))
               .ReturnsAsync([col]);

        manRepo.Setup(r => r.GetByIdAsync(col.MandateId, It.IsAny<CancellationToken>()))
              .ReturnsAsync((Mandate?)null);

        var processed = await svc.CollectDueAsync(now);

        Assert.Equal(0, processed);
        Assert.Equal(CollectionStatus.Failed, col.Status);
        Assert.Equal("MandateNotFound", col.FailureReason);

        transfer.VerifyNoOtherCalls();
        colRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CollectDue_Fails_WhenMandateNotActive()
    {
        var (svc, colRepo, manRepo, transfer) = Create();

        var now = DateTime.UtcNow;

        var mandate = new Mandate
        {
            Id = Guid.NewGuid(),
            Status = MandateStatus.Cancelled,
            PayerAccountId = Guid.NewGuid(),
            SettlementAccountId = Guid.NewGuid()
        };

        var col = new Collection
        {
            Id = Guid.NewGuid(),
            MandateId = mandate.Id,
            Status = CollectionStatus.Approved,
            DueDateUtc = now.Date,
            Amount = 100m
        };

        colRepo.Setup(r => r.GetDueForCollectionAsync(now, It.IsAny<CancellationToken>()))
               .ReturnsAsync([col]);

        manRepo.Setup(r => r.GetByIdAsync(mandate.Id, It.IsAny<CancellationToken>()))
              .ReturnsAsync(mandate);

        var processed = await svc.CollectDueAsync(now);

        Assert.Equal(0, processed);
        Assert.Equal(CollectionStatus.Failed, col.Status);
        Assert.Equal("MandateNotActive", col.FailureReason);

        transfer.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CollectDue_Fails_WhenSettlementAccountMissing()
    {
        var (svc, colRepo, manRepo, transfer) = Create();

        var now = DateTime.UtcNow;

        var mandate = new Mandate
        {
            Id = Guid.NewGuid(),
            Status = MandateStatus.Active,
            PayerAccountId = Guid.NewGuid(),
            SettlementAccountId = Guid.Empty
        };

        var col = new Collection
        {
            Id = Guid.NewGuid(),
            MandateId = mandate.Id,
            Status = CollectionStatus.Approved,
            DueDateUtc = now.Date,
            Amount = 100m
        };

        colRepo.Setup(r => r.GetDueForCollectionAsync(now, It.IsAny<CancellationToken>()))
               .ReturnsAsync([col]);

        manRepo.Setup(r => r.GetByIdAsync(mandate.Id, It.IsAny<CancellationToken>()))
              .ReturnsAsync(mandate);

        var processed = await svc.CollectDueAsync(now);

        Assert.Equal(0, processed);
        Assert.Equal(CollectionStatus.Failed, col.Status);
        Assert.Equal("SettlementAccountMissing", col.FailureReason);

        transfer.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CollectDue_Succeeds_WhenTransferSucceeds()
    {
        var (svc, colRepo, manRepo, transfer) = Create();

        var now = DateTime.UtcNow;

        var mandate = new Mandate
        {
            Id = Guid.NewGuid(),
            Status = MandateStatus.Active,
            PayerAccountId = Guid.NewGuid(),
            SettlementAccountId = Guid.NewGuid()
        };

        var col = new Collection
        {
            Id = Guid.NewGuid(),
            MandateId = mandate.Id,
            Status = CollectionStatus.Approved,
            DueDateUtc = now.Date,
            Amount = 100m
        };

        colRepo.Setup(r => r.GetDueForCollectionAsync(now, It.IsAny<CancellationToken>()))
               .ReturnsAsync([col]);

        manRepo.Setup(r => r.GetByIdAsync(mandate.Id, It.IsAny<CancellationToken>()))
              .ReturnsAsync(mandate);

        transfer.Setup(t => t.TransferAsync(mandate.PayerAccountId, mandate.SettlementAccountId, col.Amount, It.IsAny<CancellationToken>()))
                .ReturnsAsync(TransferResult.Ok());

        var processed = await svc.CollectDueAsync(now);

        Assert.Equal(1, processed);
        Assert.Equal(CollectionStatus.Collected, col.Status);
        Assert.NotNull(col.CollectedUtc);
        Assert.Null(col.FailureReason);
    }

    [Fact]
    public async Task CollectDue_Fails_WhenTransferFails()
    {
        var (svc, colRepo, manRepo, transfer) = Create();

        var now = DateTime.UtcNow;

        var mandate = new Mandate
        {
            Id = Guid.NewGuid(),
            Status = MandateStatus.Active,
            PayerAccountId = Guid.NewGuid(),
            SettlementAccountId = Guid.NewGuid()
        };

        var col = new Collection
        {
            Id = Guid.NewGuid(),
            MandateId = mandate.Id,
            Status = CollectionStatus.Approved,
            DueDateUtc = now.Date,
            Amount = 100m
        };

        colRepo.Setup(r => r.GetDueForCollectionAsync(now, It.IsAny<CancellationToken>()))
               .ReturnsAsync([col]);

        manRepo.Setup(r => r.GetByIdAsync(mandate.Id, It.IsAny<CancellationToken>()))
              .ReturnsAsync(mandate);

        transfer.Setup(t => t.TransferAsync(mandate.PayerAccountId, mandate.SettlementAccountId, col.Amount, It.IsAny<CancellationToken>()))
                .ReturnsAsync(TransferResult.Fail("InsufficientBalance"));

        var processed = await svc.CollectDueAsync(now);

        Assert.Equal(0, processed);
        Assert.Equal(CollectionStatus.Failed, col.Status);
        Assert.Equal("InsufficientBalance", col.FailureReason);
    }
}
