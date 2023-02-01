namespace AzCogniServ.Api.Jobs;

public interface IRecurringJob
{
    void Execute(IRecurringJobOptions? options = default);
}