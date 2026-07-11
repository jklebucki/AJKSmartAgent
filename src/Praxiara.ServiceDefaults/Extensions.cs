using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.Hosting;

public static class PraxiaraServiceDefaultsExtensions
{
    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks();

        var useOtlpExporter = !string.IsNullOrWhiteSpace(
            builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        builder.Services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(builder.Environment.ApplicationName))
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();

                if (useOtlpExporter)
                {
                    metrics.AddOtlpExporter();
                }
            })
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.Filter = context =>
                            !context.Request.Path.StartsWithSegments("/alive") &&
                            !context.Request.Path.StartsWithSegments("/health");
                    })
                    .AddHttpClientInstrumentation();

                if (useOtlpExporter)
                {
                    tracing.AddOtlpExporter();
                }
            });

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        app.MapHealthChecks("/health");
        app.MapHealthChecks("/alive", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("live")
        });

        return app;
    }
}