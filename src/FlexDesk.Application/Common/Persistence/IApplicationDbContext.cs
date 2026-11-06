using FlexDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlexDesk.Application.Common.Persistence;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Workspace> Workspaces { get; }
    DbSet<Reservation> Reservations { get; }
    DbSet<Payment> Payments { get; }
    DbSet<Incident> Incidents { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
