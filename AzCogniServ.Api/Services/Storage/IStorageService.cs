namespace AzCogniServ.Api.Services.Storage;

public interface IStorageService
{
    string ContainerName { get; }

    Task<bool> ExistsMetadataFor(string resourceName, CancellationToken cancellationToken = default);
    
    Task<Stream?> GetResourceBy(string name, CancellationToken cancellationToken = default);
    
    Task<AsyncResourceEnumerator> ListResourcesBy(string containerName, CancellationToken cancellationToken = default);
    
    Task SaveResourceMetadataBy(string resourceName, object metadata, CancellationToken cancellationToken = default);
}