using System.Text.Json;

using Praxiara.Contracts.Browser;

namespace Praxiara.ContractTests;

public sealed class BrowserContractSerializationTests
{
    [Fact]
    public void BrowserObservationRoundTripsAsJson()
    {
        var observation = new BrowserObservation(
            42,
            new Uri("https://ifs.example.test/main"),
            "Customer Orders",
            [new BrowserElement("e1", "textbox", "Order No", null, true, true)],
            [],
            [],
            "artifact-1",
            false);

        var json = JsonSerializer.Serialize(observation);
        var restored = JsonSerializer.Deserialize<BrowserObservation>(json);

        Assert.NotNull(restored);
        Assert.Equal(observation.Revision, restored.Revision);
        Assert.Equal(observation.Url, restored.Url);
        Assert.Equal(observation.Title, restored.Title);
        Assert.Single(restored.Elements);
        Assert.Equal(observation.Elements[0], restored.Elements[0]);
    }
}