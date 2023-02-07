namespace AzCogniServ.Api.Jobs.RecognizeImages;

public sealed record RecognizeImagesJobOptions(string Schedule) : IRecurringJobOptions;