using FlexDesk.Domain.Enums;

namespace FlexDesk.Application.Workspaces.Dtos;

public record UpdateWorkspaceRequest(
    string Name,
    WorkspaceType Type,
    int Capacity,
    decimal PricePerHour,
    bool IsActive,
    string City,
    string Street,
    string BuildingNumber,
    int Floor);
