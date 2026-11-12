using FlexDesk.Domain.Enums;

namespace FlexDesk.Application.Incidents.Dtos;

public record UpdateIncidentStatusRequest(IncidentStatus Status);
