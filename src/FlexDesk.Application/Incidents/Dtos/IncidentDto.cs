using FlexDesk.Domain.Enums;

namespace FlexDesk.Application.Incidents.Dtos;

public record IncidentDto(
    Guid Id,
    Guid WorkspaceId,
    string WorkspaceName,
    Guid UserId,
    string UserFullName,
    string Description,
    IncidentStatus Status,
    DateTime CreatedAt,
    DateTime? ResolvedAt);
