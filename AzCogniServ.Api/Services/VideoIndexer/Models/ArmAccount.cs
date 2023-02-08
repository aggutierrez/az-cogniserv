using System.Text.Json.Serialization;

namespace AzCogniServ.Api.Services.VideoIndexer.Models;

public record ArmAccount
{
    [JsonPropertyName("properties")]
    public ArmAccountProperties Properties { get; init; } = new();

    [JsonPropertyName("location")]
    public string Location { get; init; } = string.Empty;
}