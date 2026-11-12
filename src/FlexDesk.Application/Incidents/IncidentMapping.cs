using FlexDesk.Application.Incidents.Dtos;
using FlexDesk.Domain.Entities;

namespace FlexDesk.Application.Incidents;

public static class IncidentMapping
{
    public static IncidentDto ToDto(this Incident incident) =>
        new(
            incident.Id,
            incident.WorkspaceId,
            incident.Workspace.Name,
            incident.UserId,
            incident.Description,
            incident.Status,
            incident.CreatedAt,
            incident.ResolvedAt);
}
