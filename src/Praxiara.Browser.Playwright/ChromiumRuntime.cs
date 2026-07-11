using Microsoft.Playwright;

namespace Praxiara.Browser.Playwright;

public sealed class ChromiumRuntime(ChromiumRuntimeOptions options) : IAsyncDisposable
{
    private readonly SemaphoreSlim _initializationLock = new(1, 1);
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public async ValueTask<IBrowser> GetBrowserAsync(CancellationToken cancellationToken)
    {
        if (_browser is not null)
        {
            return _browser;
        }

        await _initializationLock.WaitAsync(cancellationToken);
        try
        {
            if (_browser is not null)
            {
                return _browser;
            }

            _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = options.Headless,
                ChromiumSandbox = options.ChromiumSandbox,
                ExecutablePath = options.ExecutablePath
            });

            return _browser;
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_browser is not null)
        {
            await _browser.CloseAsync();
        }

        _playwright?.Dispose();
        _initializationLock.Dispose();
    }
}