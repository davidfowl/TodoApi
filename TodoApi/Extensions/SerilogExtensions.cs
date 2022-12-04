using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Exceptions;

namespace TodoApi;

public static class SerilogExtensions
{
    public static WebApplicationBuilder AddSerilog(
        this WebApplicationBuilder builder,
        string sectionName = "Serilog")
    {
        builder.Services.AddOptions<SerilogOptions>()
            .BindConfiguration(sectionName)
            .ValidateDataAnnotations();

        builder.Host.UseSerilog((context, serviceProvider, loggerConfiguration) =>
        {
            var loggerOptions = serviceProvider.GetRequiredService<IOptions<SerilogOptions>>().Value;

            // https://github.com/serilog/serilog-settings-configuration
            loggerConfiguration.ReadFrom.Configuration(context.Configuration, sectionName: sectionName);

            loggerConfiguration
                .Enrich.WithProperty("Application", builder.Environment.ApplicationName)
                .Enrich.FromLogContext()
                .Enrich.WithEnvironmentName()
                .Enrich.WithMachineName()
                // https://rehansaeed.com/logging-with-serilog-exceptions/
                .Enrich.WithExceptionDetails();

            if (loggerOptions.UseConsole)
            {
                // https://github.com/serilog/serilog-sinks-async
                loggerConfiguration.WriteTo.Async(writeTo =>
                    writeTo.Console(outputTemplate: loggerOptions.LogTemplate));
            }

            if (!string.IsNullOrEmpty(loggerOptions.ElasticSearchUrl))
            {
                // https://github.com/serilog-contrib/serilog-sinks-elasticsearch
                loggerConfiguration.WriteTo.Async(
                    writeTo =>
                        writeTo.Elasticsearch(new(new Uri(loggerOptions.ElasticSearchUrl))
                        {
                            AutoRegisterTemplate = true,
                            IndexFormat = builder.Environment.ApplicationName
                        }));
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
}

public class SerilogOptions
{
    public bool UseConsole { get; set; } = true;
    public string? SeqUrl { get; set; } = default!;
    public string? ElasticSearchUrl { get; set; } = default!;

    public string LogTemplate { get; set; } =
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level} - {Message:lj}{NewLine}{Exception}";

    public string? LogPath { get; set; } = default!;
}