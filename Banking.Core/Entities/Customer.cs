namespace Banking.Core.Entities;

public class Customer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public CustomerType Type { get; set; } = CustomerType.Standard;

    public ICollection<Account> Accounts { get; set; } = new List<Account>();
}

public enum CustomerType
{
    Standard = 0,
    Premium = 1
}