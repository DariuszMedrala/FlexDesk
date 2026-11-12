using FlexDesk.Domain.Enums;

namespace FlexDesk.Application.Reservations;

public static class ReservationPolicy
{
    public const int CancellationCutoffHours = 24;

    public static TimeSpan MinimumDuration(WorkspaceType type) => type switch
    {
        WorkspaceType.HotDesk => TimeSpan.FromHours(1),
        WorkspaceType.MeetingRoom => TimeSpan.FromHours(2),
        WorkspaceType.Office => TimeSpan.FromHours(4),
        _ => TimeSpan.FromHours(1)
    };
}
