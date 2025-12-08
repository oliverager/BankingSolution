namespace Banking.Infrastructure;

public class DbInitializer: IDbInitializer
{
    public void Initialize(BankingDbContext context)
    {
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
    }
}

public interface IDbInitializer
{
    void Initialize(BankingDbContext context);
}