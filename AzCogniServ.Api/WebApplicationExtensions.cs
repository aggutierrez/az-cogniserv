using AzCogniServ.Api.Jobs;
using AzCogniServ.Api.Jobs.RecognizeImages;
using AzCogniServ.Api.Jobs.RecognizeVideos;
using AzCogniServ.Api.Services.Cognitive;
using AzCogniServ.Api.Services.Storage;
using AzCogniServ.Api.Services.VideoIndexer;
using Hangfire;
using Microsoft.Extensions.Azure;

namespace AzCogniServ.Api;

public static class WebApplicationExtensions
{
    public static IServiceCollection AddBackgroundJobs(this IServiceCollection services)
    {
        services.AddScoped<RecognizeImagesJob>()
            .AddScoped<RecognizeVideosJob>();

        return services;
    }

    public static IServiceCollection AddStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IStorageService, StorageService>()
            .Configure<StorageServiceOptions>(configuration.GetSection(StorageServiceOptions.ConfigKey))
            .AddSingleton<ICognitiveService, CognitiveService>()
            .AddSingleton<IVideoIndexerService, VideoIndexerService>()
            .AddAzureClients(b =>
            {
                b.AddBlobServiceClient(configuration.GetSection(StorageServiceOptions.ConfigKey));
                b.ConfigureDefaults(configuration.GetSection("AzureDefaults"));
            });

        return services;
    }
    
    public static WebApplication UseBackgroundJobs(this WebApplication app, Action<BackgroundJobsConfigBuilder> configure)
    {
        var configBuilder = BackgroundJobsConfigBuilder.AttachedTo(app.Services);
        configure(configBuilder);

        using var scope = app.Services.CreateScope();
        var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
        
        configBuilder.ApplyRecurringOn(recurringJobManager);

        return app;
    }
}