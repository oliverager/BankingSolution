namespace Banking.Core.Transfers;

public record TransferDecision(bool Allowed, string? Reason)
{
    public static TransferDecision Allow() => new(true, null);
    public static TransferDecision Deny(string reason) => new(false, reason);
}