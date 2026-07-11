using System.Text.Json;

namespace Praxiara.Contracts.Browser;

public sealed record ProposedToolCall(
    string ToolName,
    IReadOnlyDictionary<string, JsonElement> Arguments,
    long ExpectedPageRevision,
    string Reason);