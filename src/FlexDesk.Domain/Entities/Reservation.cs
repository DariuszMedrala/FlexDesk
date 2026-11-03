using FlexDesk.Domain.Enums;

namespace FlexDesk.Domain.Entities;

public class Reservation
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid WorkspaceId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal TotalPrice { get; set; }
    public ReservationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
    public Workspace Workspace { get; set; } = null!;
    public Payment? Payment { get; set; }
}
