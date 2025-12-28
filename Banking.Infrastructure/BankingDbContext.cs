using Banking.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Banking.Infrastructure;

public class BankingDbContext : DbContext
{
    public BankingDbContext(DbContextOptions<BankingDbContext> options) : base(options) { }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    
    public DbSet<Mandate> Mandates => Set<Mandate>();
    public DbSet<Collection> Collections => Set<Collection>();

    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Customer>(b =>
        {
            b.HasKey(c => c.Id);
            b.Property(c => c.Name).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<Account>(b =>
        {
            b.HasKey(a => a.Id);
            b.Property(a => a.Iban).IsRequired().HasMaxLength(34);
            b.Property(a => a.Balance).HasPrecision(18,2);

            b.HasOne(a => a.Customer)
                .WithMany(c => c.Accounts)
                .HasForeignKey(a => a.CustomerId);
        });
        
        modelBuilder.Entity<Account>()
            .HasIndex(a => a.Iban)
            .IsUnique();
        
        modelBuilder.Entity<Transaction>(b =>
        {
            b.HasKey(t => t.Id);
            b.Property(t => t.Amount).HasPrecision(18,2);
            b.Property(t => t.Status).HasMaxLength(50);

            b.HasOne(t => t.FromAccount)
                .WithMany(a => a.OutgoingTransactions)
                .HasForeignKey(t => t.FromAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(t => t.ToAccount)
                .WithMany(a => a.IncomingTransactions)
                .HasForeignKey(t => t.ToAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Mandate>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Status).IsRequired();
            b.Property(x => x.SettlementAccountId).IsRequired();
        });
        
        modelBuilder.Entity<Collection>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Amount).HasPrecision(18,2);
            b.Property(x => x.Text).HasMaxLength(500);
            b.Property(x => x.Status).IsRequired();
        });

    }
}