using FlexDesk.Domain.Entities;
using FlexDesk.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlexDesk.Infrastructure.Persistence.Configurations;

public class WorkspaceConfiguration : IEntityTypeConfiguration<Workspace>
{
    public void Configure(EntityTypeBuilder<Workspace> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(w => w.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(w => w.Capacity)
            .IsRequired();

        builder.Property(w => w.PricePerHour)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(w => w.IsActive)
            .IsRequired();

        builder.Property<uint>("Version")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

        builder.HasData(SeedWorkspaces());
    }

    private static IEnumerable<Workspace> SeedWorkspaces() =>
    [
        new Workspace { Id = Guid.Parse("a1a1a1a1-0000-0000-0000-000000000001"), Name = "Hot Desk A1", Type = WorkspaceType.HotDesk, Capacity = 1, PricePerHour = 15.00m, IsActive = true },
        new Workspace { Id = Guid.Parse("a1a1a1a1-0000-0000-0000-000000000002"), Name = "Hot Desk A2", Type = WorkspaceType.HotDesk, Capacity = 1, PricePerHour = 15.00m, IsActive = true },
        new Workspace { Id = Guid.Parse("b2b2b2b2-0000-0000-0000-000000000001"), Name = "Meeting Room Krakow", Type = WorkspaceType.MeetingRoom, Capacity = 8, PricePerHour = 60.00m, IsActive = true },
        new Workspace { Id = Guid.Parse("b2b2b2b2-0000-0000-0000-000000000002"), Name = "Meeting Room Warsaw", Type = WorkspaceType.MeetingRoom, Capacity = 4, PricePerHour = 40.00m, IsActive = true },
        new Workspace { Id = Guid.Parse("c3c3c3c3-0000-0000-0000-000000000001"), Name = "Private Office 1", Type = WorkspaceType.Office, Capacity = 3, PricePerHour = 90.00m, IsActive = true },
        new Workspace { Id = Guid.Parse("c3c3c3c3-0000-0000-0000-000000000002"), Name = "Private Office 2", Type = WorkspaceType.Office, Capacity = 5, PricePerHour = 120.00m, IsActive = true }
    ];
}
