namespace AzCogniServ.Api.Services.VideoIndexer;

public interface IVideoIndexerService
{
    Task<VideoAnalysisResult> RecognizeFrom(Stream file, string subscriptionId, string resourceGroup, string accountName,
        CancellationToken cancellationToken = default);
}