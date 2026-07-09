using ExpenseTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Data;

public class ExpenseTrackerDbContext : DbContext
{
    public ExpenseTrackerDbContext(
        DbContextOptions<ExpenseTrackerDbContext> options)
        : base(options)
    {
    }

    public DbSet<Transaction> Transactions => Set<Transaction>();

    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ExpenseTrackerDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}