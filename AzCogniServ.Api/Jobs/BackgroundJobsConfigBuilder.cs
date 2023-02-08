using AzCogniServ.Api.Jobs.RecognizeImages;
using AzCogniServ.Api.Jobs.RecognizeVideos;
using Hangfire;
using Hangfire.Common;

namespace AzCogniServ.Api.Jobs;

public sealed class BackgroundJobsConfigBuilder
{
    private readonly IServiceProvider services;
    private readonly List<(string, string, Type, IRecurringJobOptions?)> recurringJobs = new();

    private BackgroundJobsConfigBuilder(IServiceProvider services)
    {
        this.services = services;
    }

    public static BackgroundJobsConfigBuilder AttachedTo(IServiceProvider services) => new(services);

    public BackgroundJobsConfigBuilder WithRecognizeImagesJob(IConfiguration configuration)
    {
        var options = new RecognizeImagesJobOptions(string.Empty);
        var section = configuration.GetSection(RecognizeImagesJob.ConfigKey);
        
        section.Bind(options);
        recurringJobs.Add((RecognizeImagesJob.ConfigKey, options.Schedule, typeof(RecognizeImagesJob), null));
        
        return this;
    }
    
    public BackgroundJobsConfigBuilder WithRecognizeVideosJob(IConfiguration configuration)
    {
        var options = new RecognizeVideosJobOptions(string.Empty, string.Empty, string.Empty, string.Empty, false);
        var section = configuration.GetSection(RecognizeVideosJobOptions.ConfigKey);
        
        section.Bind(options);
        recurringJobs.Add((RecognizeVideosJobOptions.ConfigKey, options.Schedule, typeof(RecognizeVideosJob), options));
        
        return this;
    }

    public void ApplyRecurringOn(IRecurringJobManager manager)
    {
        foreach (var (name, schedule, type, options) in recurringJobs)
        {
            manager.AddOrUpdate(
                name,
                Job.FromExpression(() => GetJobBy(type, services).Execute(options, default)),
                schedule);
        }
    }

    private static IRecurringJob GetJobBy(Type type, IServiceProvider services)
    {
        using var scope = services.CreateScope();
        
        return (IRecurringJob)scope.ServiceProvider.GetRequiredService(type);
    }
}