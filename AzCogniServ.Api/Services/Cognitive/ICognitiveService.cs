namespace AzCogniServ.Api.Services.Cognitive;

public interface ICognitiveService
{
    Task<AnalysisResult> RecognizeFrom(Stream file, CancellationToken cancellationToken = default);
}