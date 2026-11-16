using FlexDesk.Application.Common.Security;
using FlexDesk.Domain.Entities;
using FlexDesk.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FlexDesk.Infrastructure.Persistence.Seed;

public class DatabaseSeeder
{
    private readonly FlexDeskDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public DatabaseSeeder(FlexDeskDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await _context.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var admin = new User
        {
            Id = Guid.CreateVersion7(),
            Email = "admin@flexdesk.local",
            PasswordHash = _passwordHasher.Hash("Admin123!"),
            FirstName = "Anna",
            LastName = "Administratorska",
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow
        };

        var user = new User
        {
            Id = Guid.CreateVersion7(),
            Email = "user@flexdesk.local",
            PasswordHash = _passwordHasher.Hash("User123!"),
            FirstName = "Jan",
            LastName = "Kowalski",
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.AddRange(admin, user);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
