using ExpenseTracker.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Infrastructure.Configurations;

public sealed class ApplicationUserConfiguration :
    IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(user => user.PhoneNumber)
            .HasMaxLength(32);

        builder.HasIndex(user => user.NormalizedEmail)
            .IsUnique()
            .HasDatabaseName("EmailIndex")
            .HasFilter("[NormalizedEmail] IS NOT NULL");

        builder.HasIndex(user => user.PhoneNumber)
            .IsUnique()
            .HasFilter("[PhoneNumber] IS NOT NULL");

        builder.Property(user => user.RefreshTokenHash)
            .HasMaxLength(64);

        builder.HasIndex(user => user.RefreshTokenHash)
            .IsUnique()
            .HasFilter("[RefreshTokenHash] IS NOT NULL");
    }
}
