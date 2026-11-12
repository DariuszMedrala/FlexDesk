using FlexDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FlexDesk.Application.Common.Persistence;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Workspace> Workspaces { get; }
    DbSet<Reservation> Reservations { get; }
    DbSet<Payment> Payments { get; }
    DbSet<Incident> Incidents { get; }

    EntityEntry Entry(object entity);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
