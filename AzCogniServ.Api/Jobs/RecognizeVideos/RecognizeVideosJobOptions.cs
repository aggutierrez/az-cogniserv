namespace AzCogniServ.Api.Jobs.RecognizeVideos;

public sealed record RecognizeVideosJobOptions(
    string Schedule,
    string SubscriptionId,
    string ResourceGroup,
    string AccountName,
    bool IsTrialAccount) : IRecurringJobOptions
{
    public const string ConfigKey = "RecognizeVideos";
}