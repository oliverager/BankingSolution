using System.Text.Json.Serialization;
using Banking.Core.Entities;
using Banking.Core.Interfaces.Repositories;
using Banking.Core.Interfaces.Services;
using Banking.Core.Services;
using Banking.Infrastructure;
using Banking.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BankingDbContext>(options =>
{
    var provider = builder.Configuration["Database:Provider"] ?? "Sqlite";
    var cs = builder.Configuration.GetConnectionString("DefaultConnection");

    if (string.IsNullOrWhiteSpace(cs))
        throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection");

    switch (provider.Trim().ToLowerInvariant())
    {
        case "npgsql":
        case "postgres":
        case "postgresql":
            options.UseNpgsql(cs);
            break;

        case "sqlite":
        default:
            options.UseSqlite(cs);
            break;
    }
});


// Register initializer
builder.Services.AddScoped<IDbInitializer, DbInitializer>();

builder.Services.AddScoped<IRepository<Customer>, CustomerRepository>();
builder.Services.AddScoped<IRepository<Account>, AccountRepository>();
builder.Services.AddScoped<IRepository<Transaction>, TransactionRepository>();
builder.Services.AddScoped<ICollectionRepository, CollectionRepository>();
builder.Services.AddScoped<IMandateRepository, MandateRepository>();

builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ITransferService, TransferService>();
builder.Services.AddScoped<IMandateService, MandateService>();
builder.Services.AddScoped<ICollectionService, CollectionService>();
builder.Services.AddScoped<IBsRunService, BsRunService>();

builder.Services.AddControllers()
    .AddJsonOptions(opt => opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

if (app.Configuration.GetValue<bool>("Database:ResetAndSeedOnStartup")
    && app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var initializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    await initializer.ResetAndSeedAsync();
}

// Remove the HTTPS redirect warning in tests
if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}


app.Run();

public partial class Program
{
}