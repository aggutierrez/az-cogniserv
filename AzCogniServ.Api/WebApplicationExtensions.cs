using Hangfire;

namespace AzCogniServ.Api;

public static class WebApplicationExtensions
{
    public static WebApplication UseBackgroundJobs(this WebApplication app, Action<BackgroundJobsConfigBuilder> configure)
    {
        var configBuilder = new BackgroundJobsConfigBuilder();
        configure(configBuilder);
        
        using var scope = app.Services.CreateScope();
        var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
        
        configBuilder.ApplyRecurringOn(recurringJobManager);

        return app;
    }
}