namespace Banking.Core.Entities;

public class Creditor
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? CreditorNumber { get; set; }
    public bool IsActive { get; set; } = true;
}
