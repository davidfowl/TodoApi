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
        var jaegerEndpoint = builder.Configuration.GetValue<string>("OpenTelemetry:Endpoint") ?? "http://localhost:4317";

        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.SetResourceBuilder(resourceBuilder);

            var loggingCollectorEnabled = builder.Configuration.GetValue<bool>("OpenTelemetry:LoggingCollectorEnabled");
            if (loggingCollectorEnabled)
            {
                logging.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(jaegerEndpoint);
                });
            }
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
                   .AddAspNetCoreInstrumentation()
                   .AddHttpClientInstrumentation()
                   .AddEntityFrameworkCoreInstrumentation();

            var tracingCollectorEnabled = builder.Configuration.GetValue<bool>("OpenTelemetry:TracingCollectorEnabled");
            if (tracingCollectorEnabled)
            {
                tracing.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(jaegerEndpoint);
                });
            }
        });

        return builder;
    }
}
