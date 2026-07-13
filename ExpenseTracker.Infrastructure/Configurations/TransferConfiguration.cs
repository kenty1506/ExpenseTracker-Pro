using ExpenseTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Infrastructure.Configurations;

public class TransferConfiguration :
    IEntityTypeConfiguration<Transfer>
{
    public void Configure(
        EntityTypeBuilder<Transfer> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);

        builder.Property(x => x.TransferDate)
            .IsRequired();

        builder.Property(x => x.Notes)
            .HasMaxLength(500);

        builder.HasOne(x => x.FromAccount)
            .WithMany(x => x.OutgoingTransfers)
            .HasForeignKey(x => x.FromAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ToAccount)
            .WithMany(x => x.IncomingTransfers)
            .HasForeignKey(x => x.ToAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.UserId,
            x.TransferDate
        });

        builder.HasIndex(x => new
        {
            x.UserId,
            x.FromAccountId
        });

        builder.HasIndex(x => new
        {
            x.UserId,
            x.ToAccountId
        });
    }
}