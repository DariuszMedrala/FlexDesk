using FlexDesk.Domain.Enums;

namespace FlexDesk.Domain.Entities;

public class Workspace
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public WorkspaceType Type { get; set; }
    public int Capacity { get; set; }
    public decimal PricePerHour { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public ICollection<Incident> Incidents { get; set; } = new List<Incident>();
}
