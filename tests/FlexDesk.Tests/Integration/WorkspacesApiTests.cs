using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace FlexDesk.Tests.Integration;

public class WorkspacesApiTests : IntegrationTestBase
{
    public WorkspacesApiTests(ApiFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetWorkspaces_Anonymous_ReturnsSeededWorkspaces()
    {
        var client = CreateAnonymousClient();

        var response = await client.GetAsync("/api/workspaces");
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(6, payload.GetProperty("totalCount").GetInt32());
    }

    [Fact]
    public async Task CreateWorkspace_WithoutToken_ReturnsUnauthorized()
    {
        var client = CreateAnonymousClient();

        var response = await client.PostAsJsonAsync("/api/workspaces", new
        {
            name = "Unauthorized Desk",
            type = "HotDesk",
            capacity = 1,
            pricePerHour = 10.0
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateWorkspace_AsUser_ReturnsForbidden()
    {
        var client = await CreateUserClientAsync();

        var response = await client.PostAsJsonAsync("/api/workspaces", new
        {
            name = "Forbidden Desk",
            type = "HotDesk",
            capacity = 1,
            pricePerHour = 10.0
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateWorkspace_AsAdmin_ReturnsCreated()
    {
        var client = await CreateAdminClientAsync();

        var response = await client.PostAsJsonAsync("/api/workspaces", new
        {
            name = "Admin Created Desk",
            type = "HotDesk",
            capacity = 2,
            pricePerHour = 18.5
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
