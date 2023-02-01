using Hangfire;
using Hangfire.SqlServer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
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