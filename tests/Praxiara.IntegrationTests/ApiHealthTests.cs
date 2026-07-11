using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;

namespace Praxiara.IntegrationTests;

public sealed class ApiHealthTests : IClassFixture<WebApplicationFactory<global::Program>>
{
    private readonly HttpClient _client;

    public ApiHealthTests(WebApplicationFactory<global::Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task AliveReturnsSuccess()
    {
        var response = await _client.GetAsync(
            new Uri("/alive", UriKind.Relative),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}