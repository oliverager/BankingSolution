namespace Banking.Infrastructure;

public class DbInitializer: IDbInitializer
{
    public void Initialize(BankingDbContext context)
    {
        context.Database.EnsureCreated();
        context.Database.EnsureDeleted();
    }
}

public interface IDbInitializer
{
    void Initialize(BankingDbContext context);
}