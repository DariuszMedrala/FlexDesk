namespace FlexDesk.Application.Incidents.Dtos;

public record CreateIncidentRequest(Guid WorkspaceId, string Description);
