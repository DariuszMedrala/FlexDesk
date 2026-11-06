using FlexDesk.Application.Common.Exceptions;
using FlexDesk.Application.Common.Persistence;
using FlexDesk.Application.Workspaces.Dtos;
using FlexDesk.Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace FlexDesk.Application.Workspaces;

public class WorkspaceService : IWorkspaceService
{
    private readonly IApplicationDbContext _context;
    private readonly IValidator<CreateWorkspaceRequest> _createValidator;
    private readonly IValidator<UpdateWorkspaceRequest> _updateValidator;

    public WorkspaceService(
        IApplicationDbContext context,
        IValidator<CreateWorkspaceRequest> createValidator,
        IValidator<UpdateWorkspaceRequest> updateValidator)
    {
        _context = context;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyList<WorkspaceDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Workspaces
            .AsNoTracking()
            .OrderBy(w => w.Name)
            .Select(w => new WorkspaceDto(w.Id, w.Name, w.Type, w.Capacity, w.PricePerHour, w.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<WorkspaceDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var workspace = await _context.Workspaces
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken)
            ?? throw NotFoundException.For(nameof(Workspace), id);

        return workspace.ToDto();
    }

    public async Task<WorkspaceDto> CreateAsync(CreateWorkspaceRequest request, CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var workspace = new Workspace
        {
            Id = Guid.CreateVersion7(),
            Name = request.Name,
            Type = request.Type,
            Capacity = request.Capacity,
            PricePerHour = request.PricePerHour,
            IsActive = true
        };

        _context.Workspaces.Add(workspace);
        await _context.SaveChangesAsync(cancellationToken);

        return workspace.ToDto();
    }

    public async Task<WorkspaceDto> UpdateAsync(Guid id, UpdateWorkspaceRequest request, CancellationToken cancellationToken = default)
    {
        await _updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var workspace = await _context.Workspaces
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken)
            ?? throw NotFoundException.For(nameof(Workspace), id);

        workspace.Name = request.Name;
        workspace.Type = request.Type;
        workspace.Capacity = request.Capacity;
        workspace.PricePerHour = request.PricePerHour;
        workspace.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return workspace.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var workspace = await _context.Workspaces
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken)
            ?? throw NotFoundException.For(nameof(Workspace), id);

        var hasReservations = await _context.Reservations
            .AnyAsync(r => r.WorkspaceId == id, cancellationToken);

        if (hasReservations)
        {
            throw new ConflictException("Workspace cannot be deleted while it has reservations. Deactivate it instead.");
        }

        _context.Workspaces.Remove(workspace);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
