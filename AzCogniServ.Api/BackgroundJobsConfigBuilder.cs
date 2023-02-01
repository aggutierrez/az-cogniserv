using AzCogniServ.Api.Jobs;
using AzCogniServ.Api.Jobs.SampleRecurring;
using Hangfire;
using Hangfire.Common;

namespace AzCogniServ.Api;

public sealed class BackgroundJobsConfigBuilder
{
    private readonly List<(string, string, IRecurringJob, IRecurringJobOptions?)> recurringJobs = new();

    public BackgroundJobsConfigBuilder WithSampleRecurringJob(IConfiguration configuration)
    {
        var options = new SampleRecurringJobOptions(string.Empty);
        var section = configuration.GetSection(SampleRecurringJob.Name);
        
        section.Bind(options);
        recurringJobs.Add((SampleRecurringJob.Name, options.Schedule, new SampleRecurringJob(), null));
        
        return this;
    }

    public void ApplyRecurringOn(IRecurringJobManager manager)
    {
        foreach (var (name, schedule, job, options) in recurringJobs)
            manager.AddOrUpdate(name, Job.FromExpression(() => job.Execute(options)), schedule);
    }
}