using AwesomeAssertions;
using MenuApi.Integration.Tests.Factory;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace MenuApi.Integration.Tests;

[Collection("API Host Collection")]
public class UserProvisioningIntegrationTests(ApiTestFixture fixture)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    [Fact]
    public async Task First_Authenticated_Request_Returns_A_MenuUserId()
    {
        using var client = await fixture.GetHttpClient();

        using var response = await client.GetAsync("/api/user/me");

        await response.ShouldHaveStatusCode(HttpStatusCode.OK);

        var profile = await response.Content.ReadFromJsonAsync<UserProfileResponse>(JsonOptions);
        profile.Should().NotBeNull();
        profile!.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Authenticated_Request_Stores_User_Data_In_Database()
    {
        using var client = await fixture.GetHttpClient();

        using var response = await client.GetAsync("/api/user/me");

        await response.ShouldHaveStatusCode(HttpStatusCode.OK);

        var profile = await response.Content.ReadFromJsonAsync<UserProfileResponse>(JsonOptions);
        profile.Should().NotBeNull();
        profile!.Id.Should().BeGreaterThan(0);
        profile.AuthSubject.Should().NotBeNullOrWhiteSpace();
        profile.DisplayName.Should().NotBeNullOrWhiteSpace();
        profile.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(5));
        profile.LastSeenAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(5));
    }

    [Fact]
    public async Task Repeated_Authenticated_Requests_Return_Same_MenuUserId()
    {
        using var client1 = await fixture.GetHttpClient();
        using var client2 = await fixture.GetHttpClient();

        using var response1 = await client1.GetAsync("/api/user/me");
        using var response2 = await client2.GetAsync("/api/user/me");

        await response1.ShouldHaveStatusCode(HttpStatusCode.OK);
        await response2.ShouldHaveStatusCode(HttpStatusCode.OK);

        var profile1 = await response1.Content.ReadFromJsonAsync<UserProfileResponse>(JsonOptions);
        var profile2 = await response2.Content.ReadFromJsonAsync<UserProfileResponse>(JsonOptions);

        profile1!.Id.Should().Be(profile2!.Id);
        profile1.AuthSubject.Should().Be(profile2.AuthSubject);
        profile1.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Concurrent_Authenticated_Requests_All_Return_Same_MenuUserId()
    {
        var tasks = Enumerable.Range(0, 5).Select(async _ =>
        {
            using var client = await fixture.GetHttpClient();
            using var response = await client.GetAsync("/api/user/me");
            await response.ShouldHaveStatusCode(HttpStatusCode.OK);
            return await response.Content.ReadFromJsonAsync<UserProfileResponse>(JsonOptions);
        });

        var profiles = await Task.WhenAll(tasks);

        profiles.Should().AllSatisfy(p =>
        {
            p.Should().NotBeNull();
            p!.Id.Should().BeGreaterThan(0);
        });
        profiles.Select(p => p!.Id).Distinct().Should().HaveCount(1);
    }

    private sealed class UserProfileResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("authSubject")]
        public string AuthSubject { get; set; } = null!;

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; } = null!;

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("avatarUrl")]
        public string? AvatarUrl { get; set; }

        [JsonPropertyName("createdAtUtc")]
        public DateTime CreatedAtUtc { get; set; }

        [JsonPropertyName("lastSeenAtUtc")]
        public DateTime LastSeenAtUtc { get; set; }
    }
}
