using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace TodoApi;

public static class OpenTelemetryExtensions
{
    public static WebApplicationBuilder AddOpenTelemetry(this WebApplicationBuilder builder)
    {
        var resourceBuilder = ResourceBuilder.CreateDefault().AddService(builder.Environment.ApplicationName);

        // TODO: Setup an exporter here
        builder.Logging.AddOpenTelemetry(o =>
        {
            o.SetResourceBuilder(resourceBuilder);
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
                       c.AddEventSources("Microsoft.AspNetCore.Hosting", "System.Net.Http", "System.Net.Sockets");
                   });
        });

        // TODO: Setup an exporter here
        builder.Services.AddOpenTelemetryTracing(tracing =>
        {
            tracing.SetResourceBuilder(resourceBuilder)
                   .AddAspNetCoreInstrumentation()
                   .AddHttpClientInstrumentation()
                   .AddEntityFrameworkCoreInstrumentation();
        });

        return builder;
    }
}
