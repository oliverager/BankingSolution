namespace Banking.Core.Entities;

public class Collection
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MandateId { get; set; }
    
    public DateTime DueDateUtc { get; set; }
    
    public decimal Amount { get; set; }
    public string Text { get; set; } = string.Empty;
    
    public CollectionStatus Status { get; set; } = CollectionStatus.Created;
    
    public DateTime CreatedUtc { get; set; } =  DateTime.UtcNow;
    public DateTime? NotifiedUtc { get; set; }
    public DateTime? DecisionUtc { get; set; }
    public DateTime? CollectedUtc { get; set; }
    
    public string? FailureReason { get; set; }
}

public enum CollectionStatus
{
    Created = 0,
    Notified = 1,
    Approved = 2,
    Rejected = 3,
    Collected = 4,
    Failed = 5,
    Cancelled = 6
}