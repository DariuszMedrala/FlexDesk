using FlexDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlexDesk.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Amount)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(p => p.Method)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(p => p.PaymentDate)
            .IsRequired();

        builder.Property(p => p.IsSuccessful)
            .IsRequired();

        builder.HasOne(p => p.Reservation)
            .WithOne(r => r.Payment)
            .HasForeignKey<Payment>(p => p.ReservationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.ReservationId)
            .IsUnique();
    }
}
