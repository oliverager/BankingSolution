using Banking.Core.Entities;

namespace Banking.Api.Contracts;

public record CreateMandateRequest(Guid DebtorCustomerId, Guid PayerAccountId, Guid CreditorId, Guid SettlementAccountId);

public record MandateResponse(
    Guid Id,
    Guid DebtorCustomerId,
    Guid PayerAccountId,
    MandateStatus Status,
    DateTime CreatedUtc,
    DateTime? ActivatedUtc,
    DateTime? CancelledUtc,
    Guid SettlementAccountId);

public record CreateCollectionRequest(Guid MandateId, DateTime DueDateUtc, decimal Amount, string Text);

public record CollectionResponse(
    Guid Id,
    Guid MandateId,
    DateTime DueDateUtc,
    decimal Amount,
    string Text,
    CollectionStatus Status,
    DateTime CreatedUtc,
    DateTime? NotifiedUtc,
    DateTime? DecisionUtc,
    DateTime? CollectedUtc,
    string? FailureReason);

public static class BsMappings
{
    public static MandateResponse ToResponse(this Mandate m) =>
        new(m.Id, m.DebtorCustomerId, m.PayerAccountId, m.Status, m.CreatedUtc, m.ActivatedUtc, m.CancelledUtc, m.SettlementAccountId);

    public static CollectionResponse ToResponse(this Collection c) =>
        new(c.Id, c.MandateId, c.DueDateUtc, c.Amount, c.Text, c.Status, c.CreatedUtc, c.NotifiedUtc, c.DecisionUtc, c.CollectedUtc, c.FailureReason);
}