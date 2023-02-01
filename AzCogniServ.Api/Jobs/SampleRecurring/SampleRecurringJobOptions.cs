namespace AzCogniServ.Api.Jobs.SampleRecurring;

public sealed record SampleRecurringJobOptions(string Schedule) : IRecurringJobOptions;