using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;

namespace Praxiara.IntegrationTests;

public sealed class IfsEnvironmentEndpointSecurityTests : IClassFixture<WebApplicationFactory<global::Program>>
{
    private readonly HttpClient _client;

    public IfsEnvironmentEndpointSecurityTests(WebApplicationFactory<global::Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ListEnvironmentsDeniesRequestsWithoutConfiguredIdentity()
    {
        using var response = await _client.GetAsync(
            new Uri("/api/v1/ifs/environments", UriKind.Relative),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}