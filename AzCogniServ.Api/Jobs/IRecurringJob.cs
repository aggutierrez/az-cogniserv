namespace AzCogniServ.Api.Jobs;

public interface IRecurringJob
{
    Task Execute(IRecurringJobOptions? options = default, CancellationToken cancellationToken = default);
}