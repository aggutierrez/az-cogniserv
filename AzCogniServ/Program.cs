using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

const string compvKey = "8c82a1d3adf84c5a984b854075f2a1d9";
const string compvEndpoint = "https://kandu-comv.cognitiveservices.azure.com/";

using var client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(compvKey)) { Endpoint = compvEndpoint };
var imageUrl = "https://moderatorsampleimages.blob.core.windows.net/samples/sample16.png";

Console.WriteLine($"Analyzing image [{imageUrl}]...");

var results = await client.AnalyzeImageAsync(imageUrl,
    visualFeatures: new List<VisualFeatureTypes?>
    {
        VisualFeatureTypes.Tags,
        VisualFeatureTypes.Categories,
        VisualFeatureTypes.Color,
        VisualFeatureTypes.Adult,
        VisualFeatureTypes.Brands,
        VisualFeatureTypes.Description,
        VisualFeatureTypes.Faces,
        VisualFeatureTypes.ImageType
    });

if (results.Tags.Count == 0)
{
    Console.WriteLine("No tags found");
    return;
}

Console.WriteLine($"Found [{results.Tags.Count}] tags:");
Console.WriteLine();
Console.WriteLine("| Tag name -> Tag confidence |");

foreach (var tag in results.Tags)
    Console.WriteLine($"| {tag.Name}:   {tag.Confidence} |");

Console.WriteLine();
Console.WriteLine($"Found [{results.Categories.Count}] categories:");
Console.WriteLine();
Console.WriteLine("| Category name -> Category confidence |");

foreach (var category in results.Categories)
    Console.WriteLine($"| {category.Name}:   {category.Score} |");