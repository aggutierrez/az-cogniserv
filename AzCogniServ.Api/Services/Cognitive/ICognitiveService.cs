namespace AzCogniServ.Api.Services.Cognitive;

public interface ICognitiveService
{
    Task<ImageAnalysisResult> RecognizeFrom(Stream file, CancellationToken cancellationToken = default);
}