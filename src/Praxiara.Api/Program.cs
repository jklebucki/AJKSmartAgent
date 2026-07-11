using Praxiara.Api.Hubs;
using Praxiara.Api.Security;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();
builder.Services
    .AddAuthentication(DenyAllAuthenticationHandler.SchemeName)
    .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, DenyAllAuthenticationHandler>(
        DenyAllAuthenticationHandler.SchemeName,
        _ => { });
builder.Services.AddAuthorization();
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.MaximumReceiveMessageSize = 64 * 1024;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapDefaultEndpoints();
app.MapHub<TaskEventsHub>("/hubs/tasks");
app.MapGet("/api/v1/system/info", () => new SystemInfoResponse(
        "Praxiara Control Plane",
        typeof(Program).Assembly.GetName().Version?.ToString() ?? "0.0.0",
        "online"))
    .WithName("GetSystemInfo");

app.Run();

public partial class Program;