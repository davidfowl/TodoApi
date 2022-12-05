using Serilog;
using Serilog.Exceptions;

namespace TodoApi;

public static class SerilogExtensions
{
    public static WebApplicationBuilder AddSerilog(
        this WebApplicationBuilder builder,
        string sectionName = "Serilog")
    {
        var serilogOptions = new SerilogOptions();
        builder.Configuration.GetSection(sectionName).Bind(serilogOptions);

        builder.Host.UseSerilog((context, loggerConfiguration) =>
        {
            // https://github.com/serilog/serilog-settings-configuration
            loggerConfiguration.ReadFrom.Configuration(context.Configuration, sectionName: sectionName);

            loggerConfiguration
                .Enrich.WithProperty("Application", builder.Environment.ApplicationName)
                .Enrich.FromLogContext()
                // https://rehansaeed.com/logging-with-serilog-exceptions/
                .Enrich.WithExceptionDetails();

            if (serilogOptions.UseConsole)
            {
                // https://github.com/serilog/serilog-sinks-async
                loggerConfiguration.WriteTo.Async(writeTo =>
                    writeTo.Console(outputTemplate: serilogOptions.LogTemplate));
            }

            if (!string.IsNullOrEmpty(serilogOptions.ElasticSearchUrl))
            {
                // https://github.com/serilog-contrib/serilog-sinks-elasticsearch
                loggerConfiguration.WriteTo.Elasticsearch(new(new Uri(serilogOptions.ElasticSearchUrl))
                {
                    AutoRegisterTemplate = true,
                    IndexFormat = builder.Environment.ApplicationName
                });
            }

            if (!string.IsNullOrEmpty(serilogOptions.SeqUrl))
            {
                loggerConfiguration.WriteTo.Seq(serilogOptions.SeqUrl);
            }
        });

        return builder;
    }

    private sealed class SerilogOptions
    {
        public bool UseConsole { get; set; } = true;
        public string? SeqUrl { get; set; }
        public string? ElasticSearchUrl { get; set; }

        public string LogTemplate { get; set; } =
            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level} - {Message:lj}{NewLine}{Exception}";
    }
}