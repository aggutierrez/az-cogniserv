namespace AzCogniServ.Api.Services.VideoIndexer;

public sealed class VideoIndexerService : IVideoIndexerService
{
    public async Task<string> RecognizeFrom(Stream file, string videoName, ArmAccountDescriptor accountDescriptor,
        ILogger<VideoIndexerResourceProviderClient> logger, string? videoDescription = null, CancellationToken cancellationToken = default)
    {
        var resourceProviderClient = await VideoIndexerResourceProviderClient.Of(
            accountDescriptor.SubscriptionId,
            accountDescriptor.ResourceGroup,
            accountDescriptor.AccountName,
            logger);
        var id = await resourceProviderClient.UploadVideoFrom(
            file,
            videoName,
            videoDescription ?? $"Video named {videoName}",
            accountDescriptor.IsTrialAccount,
            cancellationToken);

        await resourceProviderClient.WaitForCompletionOf(id, accountDescriptor.IsTrialAccount, cancellationToken);
        
        return await resourceProviderClient.GetVideoBy(id, cancellationToken);
    }
}