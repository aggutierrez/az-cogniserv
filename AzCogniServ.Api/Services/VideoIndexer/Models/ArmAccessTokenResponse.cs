using System.Text.Json.Serialization;

namespace AzCogniServ.Api.Services.VideoIndexer.Models;

public record ArmAccessTokenResponse
{
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; init; }
}