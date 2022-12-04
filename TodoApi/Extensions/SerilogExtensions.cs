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
                .Enrich.WithProperty("Application", builder.Environment.ApplicationName)
                .Enrich.FromLogContext()
                .Enrich.WithEnvironmentName()
                .Enrich.WithMachineName()
                // https://rehansaeed.com/logging-with-serilog-exceptions/
                .Enrich.WithExceptionDetails();

            if (loggerOptions.UseConsole)
            {
                // https://github.com/serilog/serilog-sinks-async
                loggerConfiguration.WriteTo.Async(writeTo => writeTo.Console(
                    logLevel,
                    loggerOptions.LogTemplate
                ));
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
        "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u4}] {Message:lj}{NewLine}{Exception} {Properties:j}";
    public string? LogPath { get; set; } = default!;
}