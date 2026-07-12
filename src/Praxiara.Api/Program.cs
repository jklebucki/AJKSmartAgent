using Microsoft.EntityFrameworkCore;

using Praxiara.Api.Auth;
using Praxiara.Api.Hubs;
using Praxiara.Api.Ifs;
using Praxiara.Api.Security;
using Praxiara.Application.Abstractions;
using Praxiara.Application.Ifs;
using Praxiara.Infrastructure.Persistence;
using Praxiara.Integrations.IFS;

using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();
builder.Services.AddPraxiaraAuthentication(builder.Configuration);
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "praxiara-antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        IfsAuthorizationPolicies.EnvironmentRead,
        policy => policy.RequireAuthenticatedUser().RequireRole("praxiara-operator", "praxiara-admin"));
    options.AddPolicy(
        IfsAuthorizationPolicies.EnvironmentWrite,
        policy => policy.RequireAuthenticatedUser().RequireRole("praxiara-admin"));
});
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IfsEnvironmentAdministrationService>();
builder.Services.AddScoped<IIfsAccessTokenProvider, IfsAccessTokenProvider>();
builder.Services.AddScoped<IIfsProjectionMetadataReader, IfsProjectionMetadataReader>();
builder.Services.AddHttpClient(IfsHttpClientNames.Authentication, client => client.Timeout = TimeSpan.FromSeconds(15));
builder.Services.AddHttpClient(IfsHttpClientNames.ProjectionMetadata, client => client.Timeout = TimeSpan.FromSeconds(15));

var databaseConnectionString = builder.Configuration.GetConnectionString("Praxiara");
if (string.IsNullOrWhiteSpace(databaseConnectionString))
{
    builder.Services.AddSingleton<IIfsEnvironmentConfigurationStore, UnconfiguredIfsEnvironmentConfigurationStore>();
}
else
{
    builder.Services.AddDbContext<PraxiaraDbContext>(options => options.UseNpgsql(databaseConnectionString));
    builder.Services.AddScoped<IIfsEnvironmentConfigurationStore, IfsEnvironmentConfigurationStore>();
}
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.MaximumReceiveMessageSize = 64 * 1024;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.WithTitle("Praxiara API Reference"));
}

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapDefaultEndpoints();
app.MapHub<TaskEventsHub>("/hubs/tasks");
app.MapAuthenticationEndpoints();
app.MapIfsEnvironmentEndpoints();
app.MapGet("/api/v1/system/info", () => new SystemInfoResponse(
        "Praxiara Control Plane",
        typeof(Program).Assembly.GetName().Version?.ToString() ?? "0.0.0",
        "online"))
    .WithName("GetSystemInfo");

app.Run();

public partial class Program;