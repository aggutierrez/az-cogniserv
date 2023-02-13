using AzCogniServ.Api.Services.Storage;
using AzCogniServ.Api.Services.VideoIndexer;
using Hangfire;

namespace AzCogniServ.Api.Jobs.RecognizeVideos;

/*
 * Adapted from the samples from the Github repo: https://github.com/Azure-Samples/media-services-video-indexer.git
 * With the help of the API explorer at: https://api-portal.videoindexer.ai/api-details#api=Operations
 */
public class RecognizeVideosJob : IRecurringJob
{
    private readonly IStorageService storageService;
    private readonly IVideoIndexerService videoIndexerService;
    private readonly ILogger<RecognizeVideosJob> jobLogger;
    private readonly ILogger<VideoIndexerResourceProviderClient> clientLogger;

    public RecognizeVideosJob(IStorageService storageService, IVideoIndexerService videoIndexerService,
        ILogger<RecognizeVideosJob> jobLogger, ILogger<VideoIndexerResourceProviderClient> clientLogger)
    {
        this.storageService = storageService;
        this.videoIndexerService = videoIndexerService;
        this.jobLogger = jobLogger;
        this.clientLogger = clientLogger;
    }
    
    [AutomaticRetry(Attempts = 0)]
    public async Task Execute(IRecurringJobOptions? options = default, CancellationToken cancellationToken = default)
    {
        if (options is not RecognizeVideosJobOptions recognizeVideosOptions)
            throw new ArgumentException(nameof(options));
        
        jobLogger.LogInformation("Scanning for new videos to recognize...");

        var account = new ArmAccountDescriptor(
            recognizeVideosOptions.SubscriptionId,
            recognizeVideosOptions.ResourceGroup,
            recognizeVideosOptions.AccountName,
            recognizeVideosOptions.IsTrialAccount);
        var videos = (await storageService.ListResourcesBy(storageService.ContainerName, cancellationToken))
            .WithFileExtensions(".mp4");

        await foreach (var video in videos)
        {
            jobLogger.LogDebug("Found video with name [{Resource}]", video);
            
            if (await storageService.ExistsMetadataFor(video, cancellationToken))
            {
                jobLogger.LogDebug("File [{Video}] skips analysis as there is already a metadata file", video);
                continue;
            }
            
            await using var file = await storageService.GetResourceBy(video, cancellationToken);
            var result = await videoIndexerService.RecognizeFrom(
                file!,
                video,
                account,
                clientLogger,
                cancellationToken: cancellationToken);

            await storageService.SaveResourceMetadataBy(video, result, cancellationToken);
        }
    }
}