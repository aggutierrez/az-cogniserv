using AzCogniServ.Api.Jobs.SampleRecurring;
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

    public BackgroundJobsConfigBuilder WithSampleRecurringJob(IConfiguration configuration)
    {
        var options = new SampleRecurringJobOptions(string.Empty);
        var section = configuration.GetSection(SampleRecurringJob.ConfigKey);
        
        section.Bind(options);
        recurringJobs.Add((SampleRecurringJob.ConfigKey, options.Schedule, typeof(SampleRecurringJob), null));
        
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