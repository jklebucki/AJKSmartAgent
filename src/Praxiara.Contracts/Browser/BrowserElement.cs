namespace Praxiara.Contracts.Browser;

public sealed record BrowserElement(
    string Reference,
    string Role,
    string Name,
    string? Value,
    bool Enabled,
    bool Visible);