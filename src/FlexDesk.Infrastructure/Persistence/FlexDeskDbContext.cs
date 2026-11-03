using FlexDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlexDesk.Infrastructure.Persistence;

public class FlexDeskDbContext : DbContext
{
    public FlexDeskDbContext(DbContextOptions<FlexDeskDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Incident> Incidents => Set<Incident>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FlexDeskDbContext).Assembly);
    }
}
