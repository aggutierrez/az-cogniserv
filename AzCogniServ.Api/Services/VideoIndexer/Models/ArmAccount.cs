using System.Text.Json.Serialization;

namespace AzCogniServ.Api.Services.VideoIndexer.Models;

public record ArmAccount
{
    [JsonPropertyName("properties")]
    public ArmAccountProperties Properties { get; init; }

    [JsonPropertyName("location")]
    public string Location { get; init; }
}