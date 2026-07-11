using System.Text.Json;

namespace Praxiara.Contracts.Browser;

public sealed record ToolExecutionResult(
    bool Succeeded,
    string OutcomeCode,
    string? Message,
    long ResultingPageRevision,
    IReadOnlyDictionary<string, JsonElement>? Data = null);