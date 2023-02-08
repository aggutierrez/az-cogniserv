using AzCogniServ.Api.Services.Cognitive;
using AzCogniServ.Api.Services.Storage;
using Hangfire;

namespace AzCogniServ.Api.Jobs.RecognizeImages;

public sealed class RecognizeImagesJob : IRecurringJob
{
    public const string ConfigKey = "RecognizeImages";
    
    private readonly IStorageService storageService;
    private readonly ICognitiveService cognitiveService;
    private readonly ILogger<RecognizeImagesJob> logger;

    public RecognizeImagesJob(IStorageService storageService, ICognitiveService cognitiveService, ILogger<RecognizeImagesJob> logger)
    {
        this.storageService = storageService;
        this.cognitiveService = cognitiveService;
        this.logger = logger;
    }
    
    [AutomaticRetry(Attempts = 0)]
    public async Task Execute(IRecurringJobOptions? options = default, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Scanning for new images to recognize...");

        var images = (await storageService.ListResourcesBy(storageService.ContainerName, cancellationToken))
            .WithFileExtensions(".png", ".jpg", ".gif", ".webp", ".bmp");

        await foreach (var image in images)
        {
            logger.LogDebug("Found image with name [{Image}]", image);
            
            if (await storageService.ExistsMetadataFor(image, cancellationToken))
            {
                logger.LogDebug("File [{Image}] skips analysis as there is already a metadata file", image);
                continue;
            }

            await using var file = await storageService.GetResourceBy(image, cancellationToken);
            var result = await cognitiveService.RecognizeFrom(file!, cancellationToken);
            
            logger.LogDebug("File [{Resource}]: {Tags} / {Categories} / {Description}",
                image,
                string.Join(',', result.Tags),
                string.Join(',', result.Categories),
                result.Description);

            await storageService.SaveResourceMetadataBy(image, result, cancellationToken);
        }
    }
}