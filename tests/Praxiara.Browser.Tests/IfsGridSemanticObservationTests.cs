using Microsoft.Playwright;

using Praxiara.Browser.Playwright;
using Praxiara.TestSites;

namespace Praxiara.Browser.Tests;

public sealed class IfsGridSemanticObservationTests
{
    [Fact]
    public async Task AriaSnapshotTracksVisibleWindowWithoutExposingPageScript()
    {
        await using var runtime = new ChromiumRuntime(new ChromiumRuntimeOptions());
        var browser = await runtime.GetBrowserAsync(TestContext.Current.CancellationToken);
        var context = await browser.NewContextAsync();

        try
        {
            var page = await context.NewPageAsync();
            await page.SetContentAsync(IfsCustomerInvoiceGridPage.Html);
            var grid = page.GetByRole(AriaRole.Grid, new() { Name = "Customer invoices" });

            var initialSnapshot = await grid.AriaSnapshotAsync();

            Assert.Contains("Invoice 1001, Northwind, open, 1250.00 PLN", initialSnapshot, StringComparison.Ordinal);
            Assert.DoesNotContain("Invoice 1004, Adventure Works", initialSnapshot, StringComparison.Ordinal);
            Assert.DoesNotContain("querySelector", initialSnapshot, StringComparison.Ordinal);

            await page.GetByRole(AriaRole.Button, new() { Name = "Load next invoice window" }).ClickAsync();
            await page.GetByRole(
                    AriaRole.Row,
                    new() { Name = "Invoice 1004, Adventure Works, open, 99.99 PLN" })
                .WaitForAsync();

            var updatedSnapshot = await grid.AriaSnapshotAsync();

            Assert.Contains("Invoice 1004, Adventure Works, open, 99.99 PLN", updatedSnapshot, StringComparison.Ordinal);
            Assert.DoesNotContain("Invoice 1001, Northwind", updatedSnapshot, StringComparison.Ordinal);
            Assert.DoesNotContain("querySelector", updatedSnapshot, StringComparison.Ordinal);
        }
        finally
        {
            await context.CloseAsync();
        }
    }
}