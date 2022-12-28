using Serilog;

namespace TodoApi;

public static class SerilogExtensions
{
    public static WebApplicationBuilder AddSerilog(
        this WebApplicationBuilder builder,
        string sectionName = "Serilog")
    {
        builder.Host.UseSerilog((context, loggerConfiguration) =>
        {
            // Uncomment to debug serilog configuration source, startup errors.
            //Serilog.Debugging.SelfLog.Enable(Console.Error);

            // https://github.com/serilog/serilog-settings-configuration
            loggerConfiguration.ReadFrom.Configuration(context.Configuration, sectionName: sectionName);
        });

        return builder;
    }
}