using Banking.Core.Entities;

namespace Banking.Api.Contracts;

public class CreateCustomerRequest
{
    public string Name { get; set; } = string.Empty;
    public CustomerType Type { get; set; } = CustomerType.Standard;
}