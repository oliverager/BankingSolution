using Banking.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Banking.Infrastructure;

public interface IDbInitializer
{
    Task ResetAndSeedAsync(CancellationToken ct = default);
    Task SeedIfEmptyAsync(CancellationToken ct = default);
}

public sealed class DbInitializer : IDbInitializer
{
    private readonly BankingDbContext _db;

    // Deterministic seed clock (tests won’t break on date/time)
    private static readonly DateTime SeedNowUtc =
        new DateTime(2025, 01, 01, 12, 00, 00, DateTimeKind.Utc);

    public DbInitializer(BankingDbContext db) => _db = db;

    public async Task ResetAndSeedAsync(CancellationToken ct = default)
    {
        // Hard reset: tests/dev only (call this explicitly)
        await _db.Database.EnsureDeletedAsync(ct);
        await _db.Database.EnsureCreatedAsync(ct);
        await SeedCoreAsync(ct);
    }

    public async Task SeedIfEmptyAsync(CancellationToken ct = default)
    {
        // Safe for Docker/Postgres with volumes: don’t wipe anything
        await _db.Database.EnsureCreatedAsync(ct);

        var hasAnyData =
            await _db.Customers.AnyAsync(ct) ||
            await _db.Accounts.AnyAsync(ct) ||
            await _db.Mandates.AnyAsync(ct) ||
            await _db.Collections.AnyAsync(ct);

        if (hasAnyData) return;

        await SeedCoreAsync(ct);
    }

    private async Task SeedCoreAsync(CancellationToken ct)
    {
        // ---- FIXED IDS (tests depend on these) ----
        var custA = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var custB = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var accA = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var accALow = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaab");
        var accAInactive = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaac");
        var accB = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var accSettlement = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

        var mandateActive = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
        var mandateCancelled = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");

        // ---- Customers ----
        _db.Customers.AddRange(
            new Customer { Id = custA, Name = "Transfer User", Type = CustomerType.Standard },
            new Customer { Id = custB, Name = "Receiver User", Type = CustomerType.Standard }
        );

        // ---- Accounts ----
        _db.Accounts.AddRange(
            new Account { Id = accA, CustomerId = custA, Iban = "DKTEST0001", Balance = 10_000m, IsActive = true },
            new Account { Id = accALow, CustomerId = custA, Iban = "DKTEST0002", Balance = 50m, IsActive = true },
            new Account
            {
                Id = accAInactive, CustomerId = custA, Iban = "DKTEST0003", Balance = 10_000m, IsActive = false
            },
            new Account { Id = accB, CustomerId = custB, Iban = "DKTEST0004", Balance = 1_000m, IsActive = true },
            new Account { Id = accSettlement, CustomerId = custB, Iban = "DKTEST0999", Balance = 0m, IsActive = true }
        );

        // ---- Mandates ----
        _db.Mandates.AddRange(
            new Mandate
            {
                Id = mandateActive,
                DebtorCustomerId = custA,
                PayerAccountId = accA,
                SettlementAccountId = accSettlement,
                Status = MandateStatus.Active,
                CreatedUtc = SeedNowUtc.AddDays(-10),
                ActivatedUtc = SeedNowUtc.AddDays(-9)
            },
            new Mandate
            {
                Id = mandateCancelled,
                DebtorCustomerId = custA,
                PayerAccountId = accA,
                SettlementAccountId = accSettlement,
                Status = MandateStatus.Cancelled,
                CreatedUtc = SeedNowUtc.AddDays(-10),
                CancelledUtc = SeedNowUtc.AddDays(-5)
            }
        );

        // ---- Collections (BSRun focus) ----
        _db.Collections.AddRange();

        await _db.SaveChangesAsync(ct);
    }
}