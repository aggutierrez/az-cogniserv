namespace AzCogniServ.Api.Services.VideoIndexer;

public interface IVideoIndexerService
{
    Task<string> RecognizeFrom(Stream file, string videoName, ArmAccountDescriptor accountDescriptor,
        ILogger<VideoIndexerResourceProviderClient> logger, string? videoDescription = null, CancellationToken cancellationToken = default);
}