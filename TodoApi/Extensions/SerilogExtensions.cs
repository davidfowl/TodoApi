using System.Globalization;
using System.Reflection;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;

namespace TodoApi;

public static class SerilogExtensions
{
    public static WebApplicationBuilder AddSerilog(
        this WebApplicationBuilder builder,
        string sectionName = "Serilog",
        Action<LoggerConfiguration>? extraConfigure = null)
    {
        builder.Services.AddOptions<SerilogOptions>()
            .BindConfiguration(sectionName)
            .ValidateDataAnnotations();


        builder.Host.UseSerilog((context, serviceProvider, loggerConfiguration) =>
        {
            var loggerOptions = serviceProvider.GetRequiredService<IOptions<SerilogOptions>>().Value;

            // https://github.com/serilog/serilog-settings-configuration
            loggerConfiguration.ReadFrom.Configuration(context.Configuration, sectionName: sectionName);

            extraConfigure?.Invoke(loggerConfiguration);

            var levelString = context.Configuration.GetValue<string>($"{sectionName}:MinimumLevel:Default") ??
                        nameof(LogEventLevel.Information);

            Enum.TryParse<LogEventLevel>(levelString, true, out var logLevel);

            loggerConfiguration
                .Enrich.WithProperty("Application", "TodoApi")
                .Enrich.FromLogContext()
                .Enrich.WithEnvironmentName()
                .Enrich.WithMachineName()
                // https://rehansaeed.com/logging-with-serilog-exceptions/
                .Enrich.WithExceptionDetails();

            if (context.HostingEnvironment.IsDevelopment())
            {
                // https://github.com/serilog/serilog-sinks-async
                loggerConfiguration.WriteTo.Async(writeTo => writeTo.Console(
                    logLevel,
                    loggerOptions.LogTemplate
                ));
            }

            if (!string.IsNullOrEmpty(loggerOptions.ElasticSearchUrl))
            {
                // https://github.com/serilog-contrib/serilog-sinks-elasticsearch
                loggerConfiguration.WriteTo.Async(
                    writeTo =>
                        writeTo.Elasticsearch(ConfigureElasticSink(
                            loggerOptions.ElasticSearchUrl,
                            context.HostingEnvironment.EnvironmentName)));
            }

            if (!string.IsNullOrEmpty(loggerOptions.SeqUrl))
            {
                loggerConfiguration.WriteTo.Async(writeTo => writeTo.Seq(loggerOptions.SeqUrl));
            }

            if (!string.IsNullOrEmpty(loggerOptions.LogPath))
            {
                loggerConfiguration.WriteTo.File(
                    loggerOptions.LogPath,
                    outputTemplate: loggerOptions.LogTemplate,
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true);
            }
        });

        return builder;
    }

    private static ElasticsearchSinkOptions ConfigureElasticSink(string elasticUrl, string environment)
    {
        return new(new Uri(elasticUrl))
        {
            AutoRegisterTemplate = true,
            IndexFormat =
                $"{Assembly.GetEntryAssembly()?.GetName().Name?.ToLower(CultureInfo.InvariantCulture)
                    .Replace(".", "-", StringComparison.OrdinalIgnoreCase)}-{environment.ToLower(CultureInfo.InvariantCulture)
                    .Replace(".", "-", StringComparison.OrdinalIgnoreCase)}-{DateTime.UtcNow:yyyy-MM}"
        };
    }
}

public class SerilogOptions
{
    public string? SeqUrl { get; set; } = default!;
    public string? ElasticSearchUrl { get; set; } = default!;
    public string LogTemplate { get; set; } =
        "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u4}] {Message:lj}{NewLine}{Exception} {Properties:j}";
    public string? LogPath { get; set; } = default!;
}

// Ref: https://andrewlock.net/using-serilog-aspnetcore-in-asp-net-core-3-logging-the-selected-endpoint-name-with-serilog/
public static class RequestLogEnricher
{
    public static void EnrichFromRequest(IDiagnosticContext diagnosticContext, HttpContext httpContext)
    {
        var request = httpContext.Request;

        // Set all the common properties available for every request
        diagnosticContext.Set("Host", request.Host);
        diagnosticContext.Set("Protocol", request.Protocol);
        diagnosticContext.Set("Scheme", request.Scheme);

        // Only set it if available. You're not sending sensitive data in a querystring right?!
        if(request.QueryString.HasValue)
        {
            diagnosticContext.Set("QueryString", request.QueryString.Value);
        }

        // Set the content-type of the Response at this point
        diagnosticContext.Set("ContentType", httpContext.Response.ContentType);

        // Retrieve the IEndpointFeature selected for the request
        var endpoint = httpContext.GetEndpoint();
        if (endpoint is object) // endpoint != null
        {
            diagnosticContext.Set("EndpointName", endpoint.DisplayName);
        }
    }
}