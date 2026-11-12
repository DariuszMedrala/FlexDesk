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
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken)
            ?? throw NotFoundException.For(nameof(Incident), id);

        EnsureOwnerOrAdmin(incident);

        return incident.ToDto();
    }

    public async Task<IncidentDto> CreateAsync(CreateIncidentRequest request, CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var userId = RequireUserId();

        var workspace = await _context.Workspaces
            .FirstOrDefaultAsync(w => w.Id == request.WorkspaceId, cancellationToken)
            ?? throw NotFoundException.For(nameof(Workspace), request.WorkspaceId);

        var incident = new Incident
        {
            Id = Guid.CreateVersion7(),
            WorkspaceId = workspace.Id,
            UserId = userId,
            Description = request.Description,
            Status = IncidentStatus.Reported,
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime
        };

        _context.Incidents.Add(incident);
        await _context.SaveChangesAsync(cancellationToken);

        incident.Workspace = workspace;
        return incident.ToDto();
    }

    public async Task<IncidentDto> UpdateStatusAsync(Guid id, UpdateIncidentStatusRequest request, CancellationToken cancellationToken = default)
    {
        await _updateStatusValidator.ValidateAndThrowAsync(request, cancellationToken);

        var incident = await _context.Incidents
            .Include(i => i.Workspace)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken)
            ?? throw NotFoundException.For(nameof(Incident), id);

        if (incident.Status == IncidentStatus.Resolved)
        {
            throw new BusinessRuleException("A resolved incident can no longer change status.");
        }

        incident.Status = request.Status;
        incident.ResolvedAt = request.Status == IncidentStatus.Resolved
            ? _timeProvider.GetUtcNow().UtcDateTime
            : null;

        await _context.SaveChangesAsync(cancellationToken);

        return incident.ToDto();
    }

    private Guid RequireUserId() =>
        _currentUser.UserId ?? throw new UnauthorizedException("Authentication is required.");

    private void EnsureOwnerOrAdmin(Incident incident)
    {
        var userId = RequireUserId();
        if (!_currentUser.IsAdmin && incident.UserId != userId)
        {
            throw new ForbiddenException("You do not have access to this incident.");
        }
    }
}
