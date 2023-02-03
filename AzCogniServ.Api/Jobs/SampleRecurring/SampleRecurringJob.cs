using AzCogniServ.Api.Services.Cognitive;
using AzCogniServ.Api.Services.Storage;

namespace AzCogniServ.Api.Jobs.SampleRecurring;

public sealed class SampleRecurringJob : IRecurringJob
{
    public const string ConfigKey = nameof(SampleRecurringJob);
    
    private readonly IStorageService storageService;
    private readonly ICognitiveService cognitiveService;
    private readonly ILogger<SampleRecurringJob> logger;

    public SampleRecurringJob(IStorageService storageService, ICognitiveService cognitiveService, ILogger<SampleRecurringJob> logger)
    {
        this.storageService = storageService;
        this.cognitiveService = cognitiveService;
        this.logger = logger;
    }
    
    public async Task Execute(IRecurringJobOptions? options = default, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Scanning for new files to recognize...");

        var resources = await storageService.ListResourcesBy(storageService.ContainerName, cancellationToken);

        await foreach (var resourceName in resources)
        {
            logger.LogDebug("Found resource with name [{Resource}]", resourceName);
            
            if (await storageService.ExistsMetadataFor(resourceName, cancellationToken))
            {
                logger.LogDebug("File [{Resource}] skips analysis as there is already a metadata file", resourceName);
                continue;
            }

            var file = await storageService.GetResourceBy(resourceName, cancellationToken);
            var result = await cognitiveService.RecognizeFrom(file!, cancellationToken);
            
            logger.LogDebug("File [{Resource}]: {Tags} / {Categories} / {Description}",
                resourceName,
                string.Join(',', result.Tags),
                string.Join(',', result.Categories),
                result.Description);

            await storageService.SaveResourceMetadataBy(resourceName, result, cancellationToken);
        }
    }
}