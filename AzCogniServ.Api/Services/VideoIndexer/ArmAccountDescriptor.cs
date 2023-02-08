namespace AzCogniServ.Api.Services.VideoIndexer;

public record ArmAccountDescriptor(string SubscriptionId, string ResourceGroup, string AccountName, bool IsTrialAccount);