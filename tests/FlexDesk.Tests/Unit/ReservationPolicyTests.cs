using FlexDesk.Application.Reservations;
using FlexDesk.Domain.Enums;

namespace FlexDesk.Tests.Unit;

public class ReservationPolicyTests
{
    private static readonly DateTime Now = new(2026, 11, 20, 10, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(WorkspaceType.HotDesk, 1)]
    [InlineData(WorkspaceType.MeetingRoom, 2)]
    [InlineData(WorkspaceType.Office, 4)]
    public void MinimumDuration_ReturnsExpectedHoursPerType(WorkspaceType type, int expectedHours)
    {
        Assert.Equal(TimeSpan.FromHours(expectedHours), ReservationPolicy.MinimumDuration(type));
    }

    [Fact]
    public void MeetsMinimumDuration_MeetingRoomTooShort_ReturnsFalse()
    {
        Assert.False(ReservationPolicy.MeetsMinimumDuration(WorkspaceType.MeetingRoom, TimeSpan.FromHours(1)));
    }

    [Fact]
    public void MeetsMinimumDuration_MeetingRoomAtMinimum_ReturnsTrue()
    {
        Assert.True(ReservationPolicy.MeetsMinimumDuration(WorkspaceType.MeetingRoom, TimeSpan.FromHours(2)));
    }

    [Fact]
    public void CanCancel_MoreThan24HoursBefore_ReturnsTrue()
    {
        Assert.True(ReservationPolicy.CanCancel(Now.AddHours(25), Now, isAdmin: false));
    }

    [Fact]
    public void CanCancel_WithinCutoff_ReturnsFalse()
    {
        Assert.False(ReservationPolicy.CanCancel(Now.AddHours(23), Now, isAdmin: false));
    }

    [Fact]
    public void CanCancel_ExactlyAtCutoff_ReturnsFalse()
    {
        Assert.False(ReservationPolicy.CanCancel(Now.AddHours(24), Now, isAdmin: false));
    }

    [Fact]
    public void CanCancel_AdminWithinCutoff_ReturnsTrue()
    {
        Assert.True(ReservationPolicy.CanCancel(Now.AddHours(1), Now, isAdmin: true));
    }
}
