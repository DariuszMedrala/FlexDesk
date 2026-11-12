using FlexDesk.Application.Common.Models;
using FlexDesk.Application.Workspaces.Dtos;

namespace FlexDesk.Application.Workspaces;

public interface IWorkspaceService
{
    Task<PagedResult<WorkspaceDto>> GetAsync(WorkspaceQuery query, CancellationToken cancellationToken = default);

    Task<WorkspaceDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<WorkspaceDto> CreateAsync(CreateWorkspaceRequest request, CancellationToken cancellationToken = default);

    Task<WorkspaceDto> UpdateAsync(Guid id, UpdateWorkspaceRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
