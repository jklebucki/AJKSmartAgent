namespace Praxiara.Browser.Playwright;

public sealed record ChromiumRuntimeOptions(
    bool Headless = true,
    bool ChromiumSandbox = true,
    string? ExecutablePath = null);