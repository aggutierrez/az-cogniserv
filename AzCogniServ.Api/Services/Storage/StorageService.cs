using System.Text.Json;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;

namespace AzCogniServ.Api.Services.Storage;

public sealed class StorageService : IStorageService
{
    public const string BlobContainerName = "activities";
    
    private readonly BlobServiceClient serviceClient;
    private readonly ILogger<StorageService> logger;

    public StorageService(BlobServiceClient serviceClient, ILogger<StorageService> logger)
    {
        this.serviceClient = serviceClient;
        this.logger = logger;
    }
    
    public async Task<Stream?> GetResourceBy(string name, CancellationToken cancellationToken = default)
    {
        var containerClient = await GetBlobContainerClient(cancellationToken);

        try
        {
            return await containerClient.GetBlockBlobClient(name)
                .OpenReadAsync(cancellationToken: cancellationToken);
        }
        catch (RequestFailedException e)
        {
            if (e.Status == StatusCodes.Status404NotFound)
                return null;
            
            logger.LogError(e, "Failed to download resource");
            throw;
        }
    }

    public async Task<AsyncResourceEnumerator> ListResourcesBy(string containerName, CancellationToken cancellationToken = default)
    {
        var containerClient = await GetBlobContainerClient(cancellationToken);
        var items = containerClient.GetBlobsAsync(cancellationToken: cancellationToken);

        return new AsyncResourceEnumerator(items);
    }

    public async Task SaveResourceMetadataBy(string resourceName, object metadata, CancellationToken cancellationToken = default)
    {
        var containerClient = await GetBlobContainerClient(cancellationToken);

        await containerClient.UploadBlobAsync(
            $"{Path.GetFileNameWithoutExtension(resourceName)}.meta.json",
            BinaryData.FromString(JsonSerializer.Serialize(metadata)),
            cancellationToken);
    }

    private async Task<BlobContainerClient> GetBlobContainerClient(CancellationToken cancellationToken = default)
    {
        var containerClient = serviceClient.GetBlobContainerClient(BlobContainerName);
        
        // ReSharper disable once InvertIf
        if (!await containerClient.ExistsAsync(cancellationToken))
        {
            const string message = $"Unable to find the container [{BlobContainerName}]";
            
            logger.LogError(message);
            throw new InvalidOperationException(message);
        }
        
        return containerClient;
    }
}