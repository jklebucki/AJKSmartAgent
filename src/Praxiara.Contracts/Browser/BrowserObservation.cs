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