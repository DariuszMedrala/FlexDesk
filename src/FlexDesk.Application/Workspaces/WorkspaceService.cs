using FlexDesk.Application.Common.Exceptions;
using FlexDesk.Application.Common.Models;
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

    public async Task<PagedResult<WorkspaceDto>> GetAsync(WorkspaceQuery query, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var workspaces = _context.Workspaces.AsNoTracking();

        if (query.Type.HasValue)
        {
            workspaces = workspaces.Where(w => w.Type == query.Type.Value);
        }

        if (query.MinCapacity.HasValue)
        {
            workspaces = workspaces.Where(w => w.Capacity >= query.MinCapacity.Value);
        }

        if (query.IsActive.HasValue)
        {
            workspaces = workspaces.Where(w => w.IsActive == query.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            workspaces = workspaces.Where(w => w.Name.ToLower().Contains(search));
        }

        var totalCount = await workspaces.CountAsync(cancellationToken);

        var items = await workspaces
            .OrderBy(w => w.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(w => new WorkspaceDto(
                w.Id,
                w.Name,
                w.Type,
                w.Capacity,
                w.PricePerHour,
                w.IsActive,
                w.City,
                w.Street,
                w.BuildingNumber,
                w.Floor))
            .ToListAsync(cancellationToken);

        return new PagedResult<WorkspaceDto>(items, page, pageSize, totalCount);
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
            IsActive = true,
            City = request.City,
            Street = request.Street,
            BuildingNumber = request.BuildingNumber,
            Floor = request.Floor
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
        workspace.City = request.City;
        workspace.Street = request.Street;
        workspace.BuildingNumber = request.BuildingNumber;
        workspace.Floor = request.Floor;

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
