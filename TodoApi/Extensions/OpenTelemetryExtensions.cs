using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace TodoApi;

public static class OpenTelemetryExtensions
{

    /// <summary>
    /// Configures logging, distributed tracing, and metrics
    /// <list type="bullet">
    /// <item><term>Distributed tracing</term> uses the OTLP Exporter, which can be viewed with Jaeger</item>
    /// <item><term>Metrics</term> uses the Prometheus Exporter</item>
    /// <item><term>Logging</term> can use the OTLP Exporter, but due to limited vendor support it is not enabled by default</item>
    /// </list>
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static WebApplicationBuilder AddOpenTelemetry(this WebApplicationBuilder builder)
    {
        var resourceBuilder = ResourceBuilder.CreateDefault().AddService(builder.Environment.ApplicationName);
        var endpoint = builder.Configuration.GetValue<string>("JAEGER_ENDPOINT") ?? "http://localhost:4317";

        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging
                .SetResourceBuilder(resourceBuilder)
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(endpoint);
                });
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
                           "Microsoft-AspNetCore-Server-Kestrel",
                           "System.Net.Http",
                           "System.Net.Sockets",
                           "System.Net.NameResolution",
                           "System.Net.Security");
                   });
        });

        builder.Services.AddOpenTelemetryTracing(tracing =>
        {
            tracing.SetResourceBuilder(resourceBuilder)
                   .AddOtlpExporter(options =>
                   {
                       options.Endpoint = new Uri(endpoint);
                   })
                   .AddAspNetCoreInstrumentation()
                   .AddHttpClientInstrumentation()
                   .AddEntityFrameworkCoreInstrumentation();
        });

        return builder;
    }
}
