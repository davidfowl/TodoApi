using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace TodoApi;

public static class OpenTelemetryExtensions
{
    // Configures logging, distributed tracing, and metrics (with a prometheus endpoint)
    // Other exporters can be configured to send telemetry to.
    public static WebApplicationBuilder AddOpenTelemetry(this WebApplicationBuilder builder)
    {
        var resourceBuilder = ResourceBuilder.CreateDefault().AddService(builder.Environment.ApplicationName);

        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.SetResourceBuilder(resourceBuilder)
                   .AddOtlpExporter()
                   .AddConsoleExporter();
        });

        builder.Services.AddOpenTelemetryMetrics(metrics =>
        {
            metrics.SetResourceBuilder(resourceBuilder)
                   .AddPrometheusExporter()
                   .AddAspNetCoreInstrumentation()
                   .AddRuntimeInstrumentation()
                   .AddHttpClientInstrumentation()
                   .AddEventCountersInstrumentation(c =>
                   {
                       // https://learn.microsoft.com/en-us/dotnet/core/diagnostics/available-counters
                       c.AddEventSources(
                           "Microsoft.AspNetCore.Hosting",
                           // There's currently a bug preventing this from working
                           // "Microsoft-AspNetCore-Server-Kestrel"
                           "System.Net.Http",
                           "System.Net.Sockets",
                           "System.Net.NameResolution",
                           "System.Net.Security");
                   });
        });

        builder.Services.AddOpenTelemetryTracing(tracing =>
        {
            tracing.SetResourceBuilder(resourceBuilder)
                   .AddOtlpExporter()
                   .AddAspNetCoreInstrumentation()
                   .AddHttpClientInstrumentation()
                   .AddEntityFrameworkCoreInstrumentation();
        });

        return builder;
    }
}
