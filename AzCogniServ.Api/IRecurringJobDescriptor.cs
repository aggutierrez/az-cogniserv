using Hangfire.Common;

namespace AzCogniServ.Api;

public interface IRecurringJobDescriptor
{
    string Name { get; }
    
    string Schedule { get; }

    Job Implementation { get; }
}