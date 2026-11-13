using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace FlexDesk.Tests.Integration;

public class ReservationsApiTests : IntegrationTestBase
{
    private static readonly Guid MeetingRoom = Guid.Parse("b2b2b2b2-0000-0000-0000-000000000001");
    private static readonly Guid HotDeskA1 = Guid.Parse("a1a1a1a1-0000-0000-0000-000000000001");
    private static readonly Guid HotDeskA2 = Guid.Parse("a1a1a1a1-0000-0000-0000-000000000002");

    public ReservationsApiTests(ApiFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CreateReservation_BelowMinimumDuration_ReturnsBadRequest()
    {
        var client = await CreateUserClientAsync();
        var start = DateTime.UtcNow.AddHours(300);

        var response = await client.PostAsJsonAsync("/api/reservations", new
        {
            workspaceId = MeetingRoom,
            startTime = start,
            endTime = start.AddHours(1)
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateReservation_OverlappingSlot_ReturnsConflict()
    {
        var client = await CreateUserClientAsync();
        var start = DateTime.UtcNow.AddHours(100);

        var first = await client.PostAsJsonAsync("/api/reservations", new
        {
            workspaceId = HotDeskA2,
            startTime = start,
            endTime = start.AddHours(2)
        });
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var overlapping = await client.PostAsJsonAsync("/api/reservations", new
        {
            workspaceId = HotDeskA2,
            startTime = start.AddHours(1),
            endTime = start.AddHours(3)
        });

        Assert.Equal(HttpStatusCode.Conflict, overlapping.StatusCode);
    }

    [Fact]
    public async Task CreateReservation_EightHours_AppliesTenPercentDiscount()
    {
        var client = await CreateUserClientAsync();
        var start = DateTime.UtcNow.AddHours(200);

        var response = await client.PostAsJsonAsync("/api/reservations", new
        {
            workspaceId = HotDeskA1,
            startTime = start,
            endTime = start.AddHours(8)
        });
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(108m, payload.GetProperty("totalPrice").GetDecimal());
    }

    [Fact]
    public async Task CancelReservation_WithinCutoff_ReturnsBadRequest()
    {
        var client = await CreateUserClientAsync();
        var start = DateTime.UtcNow.AddHours(2);

        var create = await client.PostAsJsonAsync("/api/reservations", new
        {
            workspaceId = HotDeskA1,
            startTime = start,
            endTime = start.AddHours(2)
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        var created = await create.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetGuid();

        var cancel = await client.PostAsync($"/api/reservations/{id}/cancel", null);

        Assert.Equal(HttpStatusCode.BadRequest, cancel.StatusCode);
    }
}
