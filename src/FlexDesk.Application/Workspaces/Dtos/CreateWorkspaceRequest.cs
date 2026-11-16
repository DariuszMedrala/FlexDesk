using FlexDesk.Domain.Enums;

namespace FlexDesk.Application.Workspaces.Dtos;

public record CreateWorkspaceRequest(
    string Name,
    WorkspaceType Type,
    int Capacity,
    decimal PricePerHour,
    string City,
    string Street,
    string BuildingNumber,
    int Floor);
