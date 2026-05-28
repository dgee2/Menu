using AwesomeAssertions;
using MenuApi.Integration.Tests.Factory;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace MenuApi.Integration.Tests;

[Collection("API Host Collection")]
public class UserProvisioningIntegrationTests(ApiTestFixture fixture)
{
    [Fact]
    public async Task First_Authenticated_Request_Returns_A_MenuUserId()
    {
        using var client = await fixture.GetHttpClient();

        using var response = await client.GetAsync("/api/user/me");

        await response.ShouldHaveStatusCode(HttpStatusCode.OK);

        var menuUserId = await response.Content.ReadFromJsonAsync<int>();
        menuUserId.Should().BeGreaterThan(0);
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

        var id1 = await response1.Content.ReadFromJsonAsync<int>();
        var id2 = await response2.Content.ReadFromJsonAsync<int>();

        id1.Should().Be(id2);
        id1.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Concurrent_Authenticated_Requests_All_Return_Same_MenuUserId()
    {
        var tasks = Enumerable.Range(0, 5).Select(async _ =>
        {
            using var client = await fixture.GetHttpClient();
            using var response = await client.GetAsync("/api/user/me");
            await response.ShouldHaveStatusCode(HttpStatusCode.OK);
            return await response.Content.ReadFromJsonAsync<int>();
        });

        var ids = await Task.WhenAll(tasks);

        ids.Should().AllSatisfy(id => id.Should().BeGreaterThan(0));
        ids.Distinct().Should().HaveCount(1);
    }
}
