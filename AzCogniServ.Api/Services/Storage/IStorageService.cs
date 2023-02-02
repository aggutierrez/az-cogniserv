namespace AzCogniServ.Api.Services.Storage;

public interface IStorageService
{
    Task<Stream?> GetResourceBy(string name, CancellationToken cancellationToken = default);
    
    Task<AsyncResourceEnumerator> ListResourcesBy(string containerName, CancellationToken cancellationToken = default);
    
    Task SaveResourceMetadataBy(string resourceName, object metadata, CancellationToken cancellationToken = default);
}