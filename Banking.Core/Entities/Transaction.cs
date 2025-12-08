using System.ComponentModel.DataAnnotations.Schema;

namespace Banking.Core.Entities;

public class Transaction
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid FromAccountId { get; set; }
    public Guid ToAccountId { get; set; }

    public decimal Amount { get; set; }
    public DateTime TimestampUtc { get; set; }
    public string Status { get; set; } = "Completed";

    [ForeignKey(nameof(FromAccountId))]
    [InverseProperty(nameof(Account.OutgoingTransactions))]
    public Account? FromAccount { get; set; }
    
    [ForeignKey(nameof(ToAccountId))]
    [InverseProperty(nameof(Account.IncomingTransactions))]
    public Account? ToAccount { get; set; }
}