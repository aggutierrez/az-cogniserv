namespace AzCogniServ.Api.Services.VideoIndexer;

public sealed class VideoIndexerService : IVideoIndexerService
{
    public async Task<VideoAnalysisResult> RecognizeFrom(Stream file, string subscriptionId, string resourceGroup, string accountName,
        CancellationToken cancellationToken = default)
    {
        var resourceProviderClient = await VideoIndexerResourceProviderClient.Of(subscriptionId, resourceGroup, accountName);
        var id = await resourceProviderClient.UploadVideoFrom(file, cancellationToken);

        await resourceProviderClient.WaitForCompletionOf(id, cancellationToken);

        // TODO Infer response model
        Console.WriteLine(
            await resourceProviderClient.GetVideoBy(id, cancellationToken));
        
        return new VideoAnalysisResult();
    }
}