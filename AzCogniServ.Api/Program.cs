using Hangfire;
using Hangfire.SqlServer;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console();

        if (builder.Environment.IsProduction())
        {
            // Add Azure Diagnostics Log Stream
            configuration.WriteTo.File(
                Path.Combine(Environment.GetEnvironmentVariable("HOME") ?? "/", "LogFiles", "Application", "diagnostics.txt"),
                rollingInterval: RollingInterval.Day,
                fileSizeLimitBytes: 10 * 1024 * 1024,
                retainedFileCountLimit: 2,
                rollOnFileSizeLimit: true,
                shared: true,
                flushToDiskInterval: TimeSpan.FromSeconds(1));
        }
    });

    builder.Services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSerilogLogProvider()
        .UseSqlServerStorage(builder.Configuration.GetConnectionString("Default"), new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true
        })
    );

    builder.Services.AddHangfireServer();

    var app = builder.Build();

    app.UseHttpsRedirection()
        .UseRouting()
        .UseHangfireDashboard()
        .UseEndpoints(endpoints => endpoints.MapHangfireDashboard());

    using var scope = app.Services.CreateScope();
    var backgroundJobsClient = scope.ServiceProvider.GetRequiredService<IBackgroundJobClient>();

    backgroundJobsClient.Enqueue(() => Console.WriteLine("Hello world from Hangfire!"));

    app.Run();
    return 0;
}
catch (Exception e)
{
    Log.Fatal(e, "Host failed to start");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}