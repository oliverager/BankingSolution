namespace Banking.Core.Transfers;

public record TransferResult(bool Success, string? Reason)
{
    public static TransferResult Ok() => new(true, null);
    public static TransferResult Fail(string reason) => new(false, reason);
}