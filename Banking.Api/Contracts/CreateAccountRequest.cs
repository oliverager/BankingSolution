namespace Banking.Api.Contracts;

public class CreateAccountRequest
{
    public Guid CustomerId { get; set; }
    public string Iban { get; set; } = string.Empty;
    public decimal InitialBalance { get; set; }
}