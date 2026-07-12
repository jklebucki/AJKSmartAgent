using System.Text.Json;

using Praxiara.Contracts.Ifs;

namespace Praxiara.ContractTests;

public sealed class IfsEnvironmentContractSerializationTests
{
    [Fact]
    public void EnvironmentResponseDoesNotExposeSecretFilePath()
    {
        var response = new IfsEnvironmentResponse(
            "ifs-test",
            "https://ifs.example.test/",
            "tenant-test",
            "pl-PL",
            "Test",
            ["UserProfileService"],
            "BearerTokenFile",
            null,
            null,
            true);

        var json = JsonSerializer.Serialize(response);

        Assert.Contains("isSecretReferenceConfigured", json, StringComparison.Ordinal);
        Assert.DoesNotContain("secretFilePath", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("accessToken", json, StringComparison.OrdinalIgnoreCase);
    }
}