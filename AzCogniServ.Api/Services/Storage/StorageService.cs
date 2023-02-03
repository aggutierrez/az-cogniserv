using System.Text.Json;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Options;

namespace AzCogniServ.Api.Services.Storage;

public sealed class StorageService : IStorageService
{
    private readonly BlobServiceClient serviceClient;
    private readonly ILogger<StorageService> logger;
    private BlobContainerClient? client;

    public string ContainerName { get; }

    public StorageService(BlobServiceClient serviceClient, IOptions<StorageServiceOptions> options, ILogger<StorageService> logger)
    {
        this.serviceClient = serviceClient;
        this.logger = logger;
        ContainerName = options.Value.ContainerName;
    }

    public async Task<bool> ExistsMetadataFor(string resourceName, CancellationToken cancellationToken = default)
    {
        var containerClient = await GetBlobContainerClient(cancellationToken);
        
        return await containerClient.GetBlockBlobClient(BuildMetadataResourceNameFrom(resourceName))
            .ExistsAsync(cancellationToken);
    }
    
    public async Task<Stream?> GetResourceBy(string name, CancellationToken cancellationToken = default)
    {
        var containerClient = await GetBlobContainerClient(cancellationToken);

        return await containerClient.GetBlockBlobClient(name)
            .OpenReadAsync(cancellationToken: cancellationToken);
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
            BuildMetadataResourceNameFrom(resourceName),
            BinaryData.FromString(JsonSerializer.Serialize(metadata)),
            cancellationToken);
    }

    private async Task<BlobContainerClient> GetBlobContainerClient(CancellationToken cancellationToken = default)
    {
        if (client is not null)
            return client;
        
        var containerClient = serviceClient.GetBlobContainerClient(ContainerName);
        
        // ReSharper disable once InvertIf
        if (!await containerClient.ExistsAsync(cancellationToken))
        {
            var message = $"Unable to find the container [{ContainerName}]";
            
            logger.LogError(message);
            throw new InvalidOperationException(message);
        }

        client = containerClient;
        return containerClient;
    }

    private static string BuildMetadataResourceNameFrom(string resourceName) => $"{Path.GetFileNameWithoutExtension(resourceName)}.meta.json";
}