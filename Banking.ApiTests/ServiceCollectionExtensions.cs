using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Banking.ApiTests;

public static class ServiceCollectionExtensions
{
    public static void RemoveDbContext<TContext>(this IServiceCollection services)
        where TContext : DbContext
    {
        var toRemove = services
            .Where(d =>
                d.ServiceType == typeof(TContext) ||
                d.ServiceType == typeof(DbContextOptions<TContext>))
            .ToList();

        foreach (var d in toRemove)
            services.Remove(d);
    }
}