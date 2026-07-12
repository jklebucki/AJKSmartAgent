using System.Net;
using System.Text.Json;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace Praxiara.IntegrationTests;

public sealed class ApiReferenceTests : IClassFixture<WebApplicationFactory<global::Program>>
{
    private readonly WebApplicationFactory<global::Program> _factory;

    public ApiReferenceTests(WebApplicationFactory<global::Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ScalarApiReferenceReturnsHtmlInDevelopment()
    {
        using var client = _factory.CreateClient();
        using var response = await client.GetAsync(
            new Uri("/scalar/v1", UriKind.Relative),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);

        var html = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("Praxiara API Reference", html, StringComparison.Ordinal);
        Assert.Contains("openapi/v1.json", html, StringComparison.Ordinal);
        Assert.DoesNotContain("cdn.jsdelivr.net", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("proxy.scalar.com", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task OpenApiDocumentContainsSystemInfoEndpoint()
    {
        using var client = _factory.CreateClient();
        using var response = await client.GetAsync(
            new Uri("/openapi/v1.json", UriKind.Relative),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        await using var content = await response.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken);
        using var document = await JsonDocument.ParseAsync(
            content,
            cancellationToken: TestContext.Current.CancellationToken);
        var paths = document.RootElement.GetProperty("paths");

        Assert.True(paths.TryGetProperty("/api/v1/system/info", out var systemInfoPath));
        Assert.Equal("GetSystemInfo", systemInfoPath.GetProperty("get").GetProperty("operationId").GetString());
    }

    [Theory]
    [InlineData("/scalar/v1")]
    [InlineData("/openapi/v1.json")]
    public async Task ApiReferenceEndpointsAreNotExposedInProduction(string requestUri)
    {
        await using var productionFactory = _factory.WithWebHostBuilder(
            builder => builder.UseEnvironment(Environments.Production));
        using var client = productionFactory.CreateClient();
        using var response = await client.GetAsync(
            new Uri(requestUri, UriKind.Relative),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}