using System.Text.Json.Serialization;

namespace AzCogniServ.Api.Services.VideoIndexer.Models;

public record ArmVideo
{
    [JsonPropertyName("id")]
    public string Id { get; init; }

    [JsonPropertyName("state")]
    public string State { get; init; }
}