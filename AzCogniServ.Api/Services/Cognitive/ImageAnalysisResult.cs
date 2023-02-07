namespace AzCogniServ.Api.Services.Cognitive;

public record ImageAnalysisResult(IEnumerable<string> Tags, IEnumerable<string> Categories, string Description);