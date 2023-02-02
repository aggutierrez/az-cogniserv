namespace AzCogniServ.Api.Services.Cognitive;

public record AnalysisResult(IEnumerable<string> Tags, IEnumerable<string> Categories, string Description);