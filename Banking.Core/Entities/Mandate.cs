namespace Banking.Core.Entities;

public class Mandate
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid DebtorCustomerId { get; set; }
    public Guid PayerAccountId { get; set; }

    public Guid CreditorId { get; set; }
    
    public Guid SettlementAccountId { get; set; }

    public MandateStatus Status { get; set; } = MandateStatus.Pending;

    // Better: store UTC
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ActivatedUtc { get; set; }
    public DateTime? CancelledUtc { get; set; }
}

public enum MandateStatus
{
    Pending = 0,
    Active = 1,
    Cancelled = 2
}