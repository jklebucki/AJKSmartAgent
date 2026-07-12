var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => Results.Content(TestPages.IndexPage, "text/html"));
app.MapGet("/forms/basic", () => Results.Content(TestPages.BasicFormPage, "text/html"));
app.MapGet(
    "/ifs/customer-invoices",
    () => Results.Content(Praxiara.TestSites.IfsCustomerInvoiceGridPage.Html, "text/html"));
app.MapGet("/security/prompt-injection", () => Results.Content(TestPages.PromptInjectionPage, "text/html"));

app.Run();

public partial class Program;