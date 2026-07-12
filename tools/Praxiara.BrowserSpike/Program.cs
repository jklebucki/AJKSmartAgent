using System.Diagnostics;
using System.Text.Json;

using Microsoft.Playwright;

using Praxiara.Browser.Playwright;
using Praxiara.TestSites;

var cancellationToken = Console.IsInputRedirected
    ? CancellationToken.None
    : new CancellationTokenSource(TimeSpan.FromMinutes(2)).Token;
var processIdsBeforeLaunch = Process.GetProcesses().Select(process => process.Id).ToHashSet();
var startedAt = DateTimeOffset.UtcNow;
var stopwatch = Stopwatch.StartNew();

await using var runtime = new ChromiumRuntime(new ChromiumRuntimeOptions());
var browser = await runtime.GetBrowserAsync(cancellationToken);
var context = await browser.NewContextAsync();

try
{
    var page = await context.NewPageAsync();
    await page.SetContentAsync(IfsCustomerInvoiceGridPage.Html);
    var grid = page.GetByRole(AriaRole.Grid, new() { Name = "Customer invoices" });
    var snapshot = await grid.AriaSnapshotAsync();
    stopwatch.Stop();

    if (args.Contains("--snapshot-only", StringComparer.Ordinal))
    {
        Console.WriteLine(snapshot);
        return;
    }

    var browserProcesses = Process.GetProcesses()
        .Where(process => !processIdsBeforeLaunch.Contains(process.Id))
        .Where(IsBrowserProcess)
        .ToArray();
    var result = new
    {
        CollectedAtUtc = DateTimeOffset.UtcNow,
        HostOperatingSystem = Environment.OSVersion.ToString(),
        HostArchitecture = System.Runtime.InteropServices.RuntimeInformation.OSArchitecture.ToString(),
        HostProcessorCount = Environment.ProcessorCount,
        HostAvailableMemoryBytes = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes,
        PlaywrightPackageVersion = "1.61.0",
        BrowserVersion = browser.Version,
        Headless = true,
        ChromiumSandboxRequested = true,
        StartedAtUtc = startedAt,
        ReadyMilliseconds = stopwatch.ElapsedMilliseconds,
        SemanticSnapshotCharacters = snapshot.Length,
        BrowserProcessCount = browserProcesses.Length,
        BrowserResidentSetBytes = browserProcesses.Sum(TryGetWorkingSet),
        BrowserCpuMilliseconds = browserProcesses.Sum(TryGetCpuMilliseconds)
    };

    Console.WriteLine(JsonSerializer.Serialize(result, JsonSerializerOptions.Web));
}
finally
{
    await context.CloseAsync();
}

static bool IsBrowserProcess(Process process)
{
    try
    {
        return process.ProcessName.Contains("chrome", StringComparison.OrdinalIgnoreCase)
            || process.ProcessName.Contains("chromium", StringComparison.OrdinalIgnoreCase)
            || process.ProcessName.Contains("headless", StringComparison.OrdinalIgnoreCase);
    }
    catch (InvalidOperationException)
    {
        return false;
    }
}

static long TryGetWorkingSet(Process process)
{
    try
    {
        return process.WorkingSet64;
    }
    catch (InvalidOperationException)
    {
        return 0;
    }
}

static double TryGetCpuMilliseconds(Process process)
{
    try
    {
        return process.TotalProcessorTime.TotalMilliseconds;
    }
    catch (InvalidOperationException)
    {
        return 0;
    }
}