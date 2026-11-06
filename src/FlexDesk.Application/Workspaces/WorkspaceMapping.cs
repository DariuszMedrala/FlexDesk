using FlexDesk.Application.Workspaces.Dtos;
using FlexDesk.Domain.Entities;

namespace FlexDesk.Application.Workspaces;

public static class WorkspaceMapping
{
    public static WorkspaceDto ToDto(this Workspace workspace) =>
        new(
            workspace.Id,
            workspace.Name,
            workspace.Type,
            workspace.Capacity,
            workspace.PricePerHour,
            workspace.IsActive);
}
