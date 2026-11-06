using FlexDesk.Domain.Enums;

namespace FlexDesk.Application.Workspaces.Dtos;

public record WorkspaceDto(
    Guid Id,
    string Name,
    WorkspaceType Type,
    int Capacity,
    decimal PricePerHour,
    bool IsActive);
