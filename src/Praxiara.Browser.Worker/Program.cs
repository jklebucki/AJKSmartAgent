var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapDefaultEndpoints();
app.MapGet("/internal/v1/capabilities", () => new BrowserWorkerCapabilities(
        Engine: "playwright-chromium",
        SemanticObservations: true,
        Tracing: true,
        ManualTakeover: false))
    .WithName("GetBrowserWorkerCapabilities");

app.Run();

public partial class Program;