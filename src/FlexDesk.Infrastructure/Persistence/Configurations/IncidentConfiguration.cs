using FlexDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlexDesk.Infrastructure.Persistence.Configurations;

public class IncidentConfiguration : IEntityTypeConfiguration<Incident>
{
    public void Configure(EntityTypeBuilder<Incident> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(i => i.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(i => i.CreatedAt)
            .IsRequired();

        builder.Property(i => i.ResolvedAt);

        builder.HasOne(i => i.Workspace)
            .WithMany(w => w.Incidents)
            .HasForeignKey(i => i.WorkspaceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
