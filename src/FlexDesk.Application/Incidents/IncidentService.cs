using FlexDesk.Application.Common.Exceptions;
using FlexDesk.Application.Common.Persistence;
using FlexDesk.Application.Common.Security;
using FlexDesk.Application.Incidents.Dtos;
using FlexDesk.Domain.Entities;
using FlexDesk.Domain.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace FlexDesk.Application.Incidents;

public class IncidentService : IIncidentService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly TimeProvider _timeProvider;
    private readonly IValidator<CreateIncidentRequest> _createValidator;
    private readonly IValidator<UpdateIncidentStatusRequest> _updateStatusValidator;

    public IncidentService(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        TimeProvider timeProvider,
        IValidator<CreateIncidentRequest> createValidator,
        IValidator<UpdateIncidentStatusRequest> updateStatusValidator)
    {
        _context = context;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
        _createValidator = createValidator;
        _updateStatusValidator = updateStatusValidator;
    }

    public async Task<IReadOnlyList<IncidentDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var userId = RequireUserId();

        var query = _context.Incidents.AsNoTracking();

        if (!_currentUser.IsAdmin)
        {
            query = query.Where(i => i.UserId == userId);
        }

        return await query
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new IncidentDto(
                i.Id,
                i.WorkspaceId,
                i.Workspace.Name,
                i.UserId,
                i.User.FirstName + " " + i.User.LastName,
                i.Description,
                i.Status,
                i.CreatedAt,
                i.ResolvedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<IncidentDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var incident = await _context.Incidents
            .AsNoTracking()
            .Include(i => i.Workspace)
            .Include(i => i.User)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken)
            ?? throw NotFoundException.For(nameof(Incident), id);

        EnsureOwnerOrAdmin(incident);

        return incident.ToDto();
    }

    public async Task<IncidentDto> CreateAsync(CreateIncidentRequest request, CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var userId = RequireUserId();

        if (_currentUser.IsAdmin)
        {
            throw new ForbiddenException("Administrator nie może zgłaszać usterek.");
        }

        var workspace = await _context.Workspaces
            .FirstOrDefaultAsync(w => w.Id == request.WorkspaceId, cancellationToken)
            ?? throw NotFoundException.For(nameof(Workspace), request.WorkspaceId);

        var now = _timeProvider.GetUtcNow().UtcDateTime;

        var hasStartedReservation = await _context.Reservations.AnyAsync(
            r => r.UserId == userId
                && r.WorkspaceId == workspace.Id
                && r.Status != ReservationStatus.Cancelled
                && r.StartTime <= now,
            cancellationToken);

        if (!hasStartedReservation)
        {
            throw new BusinessRuleException(
                "Usterkę możesz zgłosić tylko dla przestrzeni, którą zarezerwowałeś, i dopiero po rozpoczęciu rezerwacji.");
        }

        var incident = new Incident
        {
            Id = Guid.CreateVersion7(),
            WorkspaceId = workspace.Id,
            UserId = userId,
            Description = request.Description,
            Status = IncidentStatus.Reported,
            CreatedAt = now
        };

        _context.Incidents.Add(incident);
        await _context.SaveChangesAsync(cancellationToken);

        incident.Workspace = workspace;
        incident.User = await _context.Users.FirstAsync(u => u.Id == userId, cancellationToken);
        return incident.ToDto();
    }

    public async Task<IncidentDto> UpdateStatusAsync(Guid id, UpdateIncidentStatusRequest request, CancellationToken cancellationToken = default)
    {
        await _updateStatusValidator.ValidateAndThrowAsync(request, cancellationToken);

        var incident = await _context.Incidents
            .Include(i => i.Workspace)
            .Include(i => i.User)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken)
            ?? throw NotFoundException.For(nameof(Incident), id);

        if (incident.Status == IncidentStatus.Resolved)
        {
            throw new BusinessRuleException("Rozwiązanego zgłoszenia nie można już zmienić.");
        }

        incident.Status = request.Status;
        incident.ResolvedAt = request.Status == IncidentStatus.Resolved
            ? _timeProvider.GetUtcNow().UtcDateTime
            : null;

        await _context.SaveChangesAsync(cancellationToken);

        return incident.ToDto();
    }

    private Guid RequireUserId() =>
        _currentUser.UserId ?? throw new UnauthorizedException("Wymagane jest zalogowanie.");

    private void EnsureOwnerOrAdmin(Incident incident)
    {
        var userId = RequireUserId();
        if (!_currentUser.IsAdmin && incident.UserId != userId)
        {
            throw new ForbiddenException("Nie masz dostępu do tego zgłoszenia.");
        }
    }
}
