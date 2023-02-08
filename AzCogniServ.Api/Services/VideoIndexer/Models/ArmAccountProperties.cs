using System.Text.Json.Serialization;

namespace AzCogniServ.Api.Services.VideoIndexer.Models;

public record ArmAccountProperties
{
    [JsonPropertyName("accountId")]
    public string Id { get; init; } = string.Empty;
}