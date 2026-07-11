using Praxiara.Application.Abstractions;
using Praxiara.Contracts.Browser;
using Praxiara.Policy;

namespace Praxiara.SecurityTests;

public sealed class RiskPolicyTests
{
    [Fact]
    public async Task AuthorizeAsyncDeniesUnknownTool()
    {
        var policy = new RiskPolicy(
            new Dictionary<string, ToolPolicyDescriptor>(),
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "ifs.example.test" });
        var context = new ToolPolicyContext(
            Guid.CreateVersion7(),
            "user-42",
            "test",
            new Uri("https://ifs.example.test/main"),
            new HashSet<string>());
        var call = new ProposedToolCall(
            "execute_javascript",
            new Dictionary<string, System.Text.Json.JsonElement>(),
            1,
            "Attempt to execute arbitrary code");

        var result = await policy.AuthorizeAsync(context, call, CancellationToken.None);

        Assert.False(result.Allowed);
        Assert.Contains("not present", result.DenialReason, StringComparison.Ordinal);
    }
}