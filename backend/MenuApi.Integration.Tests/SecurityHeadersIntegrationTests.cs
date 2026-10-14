using Aspire.Hosting;
using Aspire.Hosting.Testing;
using AwesomeAssertions;
using MenuApi.Integration.Tests.Factory;
using System.Net;
using Xunit;

namespace MenuApi.Integration.Tests;

[Collection("API Host Collection")]
public class SecurityHeadersIntegrationTests
{
    private readonly ApiTestFixture fixture;

    public SecurityHeadersIntegrationTests(ApiTestFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public async Task UnauthenticatedApiResponse_IncludesSecureHeaders()
    {
        using var client = fixture.app.CreateHttpClient("apiservice");
        using var response = await client.GetAsync("/api/recipe");

        await response.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);
        response.Headers.GetValues("X-Content-Type-Options").Should().ContainSingle().Which.Should().Be("nosniff");
        response.Headers.GetValues("Referrer-Policy").Should().ContainSingle().Which.Should().Be("no-referrer");
        response.Headers.GetValues("Content-Security-Policy").Should().ContainSingle();
        response.Headers.CacheControl.Should().NotBeNull();
        response.Headers.CacheControl.NoStore.Should().BeTrue();
    }

    [Fact]
    public async Task DevelopmentSwaggerUi_DoesNotReceiveApiContentSecurityPolicy()
    {
        using var client = fixture.app.CreateHttpClient("apiservice");
        using var response = await client.GetAsync("/swagger/index.html");

        await response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Headers.GetValues("X-Content-Type-Options").Should().ContainSingle().Which.Should().Be("nosniff");
        response.Headers.GetValues("Referrer-Policy").Should().ContainSingle().Which.Should().Be("no-referrer");
        response.Headers.Contains("Content-Security-Policy").Should().BeFalse();
    }
}
