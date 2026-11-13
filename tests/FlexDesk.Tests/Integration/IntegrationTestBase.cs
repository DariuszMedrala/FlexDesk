using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace FlexDesk.Tests.Integration;

[Collection(ApiCollection.Name)]
public abstract class IntegrationTestBase
{
    protected IntegrationTestBase(ApiFactory factory)
    {
        Factory = factory;
    }

    protected ApiFactory Factory { get; }

    protected HttpClient CreateAnonymousClient() => Factory.CreateClient();

    protected async Task<HttpClient> CreateAdminClientAsync() =>
        await CreateAuthenticatedClientAsync("admin@flexdesk.local", "Admin123!");

    protected async Task<HttpClient> CreateUserClientAsync() =>
        await CreateAuthenticatedClientAsync("user@flexdesk.local", "User123!");

    private async Task<HttpClient> CreateAuthenticatedClientAsync(string email, string password)
    {
        var client = Factory.CreateClient();
        var token = await LoginAsync(email, password);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private async Task<string> LoginAsync(string email, string password)
    {
        using var client = Factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        return payload.GetProperty("token").GetString()!;
    }
}
