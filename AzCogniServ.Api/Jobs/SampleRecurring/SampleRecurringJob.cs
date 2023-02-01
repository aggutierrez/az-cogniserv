namespace AzCogniServ.Api.Jobs.SampleRecurring;

public sealed class SampleRecurringJob : IRecurringJob
{
    public const string Name = "SampleJob";
    
    public void Execute(IRecurringJobOptions? options = default)
    {
        Console.WriteLine("Recurring hey there!");
    }
}