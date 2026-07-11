using System.Text.Json;

namespace Praxiara.Contracts.Browser;

public sealed record BrowserObservation(
    long Revision,
    Uri Url,
    string Title,
    IReadOnlyList<BrowserElement> Elements,
    IReadOnlyList<PageMessage> Messages,
    IReadOnlyList<PageDialog> Dialogs,
    string? ScreenshotArtifactId,
    bool DownloadInProgress);

public sealed record BrowserElement(
    string Reference,
    string Role,
    string Name,
    string? Value,
    bool Enabled,
    bool Visible);

public sealed record PageMessage(string Severity, string Text);

public sealed record PageDialog(string Kind, string Text);

public sealed record ProposedToolCall(
    string ToolName,
    IReadOnlyDictionary<string, JsonElement> Arguments,
    long ExpectedPageRevision,
    string Reason);

public sealed record ToolExecutionResult(
    bool Succeeded,
    string OutcomeCode,
    string? Message,
    long ResultingPageRevision,
    IReadOnlyDictionary<string, JsonElement>? Data = null);