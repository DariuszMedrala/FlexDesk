using FlexDesk.Domain.Enums;

namespace FlexDesk.Domain.Entities;

public class Incident
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public Guid UserId { get; set; }
    public string Description { get; set; } = string.Empty;
    public IncidentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }

    public Workspace Workspace { get; set; } = null!;
    public User User { get; set; } = null!;
}
