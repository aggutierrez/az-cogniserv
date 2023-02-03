using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace AzCogniServ.Api.Services.Cognitive;

public sealed class CognitiveService : ICognitiveService
{
    private const string CompvKey = "8c82a1d3adf84c5a984b854075f2a1d9";
    private const string CompvEndpoint = "https://kandu-comv.cognitiveservices.azure.com/";
    private const double MinConfidence = 0.0;
    
    public async Task<AnalysisResult> RecognizeFrom(Stream file, CancellationToken cancellationToken = default)
    {
        using var client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(CompvKey)) { Endpoint = CompvEndpoint };
        var results = await client.AnalyzeImageInStreamAsync(
            file,
            visualFeatures: new List<VisualFeatureTypes?>
            {
                VisualFeatureTypes.Tags,
                VisualFeatureTypes.Categories,
                VisualFeatureTypes.Description
            },
            cancellationToken: cancellationToken);

        return new AnalysisResult(
            results.Tags
                .Where(t => t.Confidence > MinConfidence)
                .Select(t => t.Name),
            results.Categories
                .Where(c => c.Score > MinConfidence)
                .Select(c => c.Name),
            results.Description?
                .Captions
                .Where(c => c.Confidence > MinConfidence)
                .MaxBy(c => c.Confidence)
                ?.Text ?? string.Empty);
    }
}