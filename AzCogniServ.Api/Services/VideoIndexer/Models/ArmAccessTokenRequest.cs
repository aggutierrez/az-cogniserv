using System.Text.Json.Serialization;

namespace AzCogniServ.Api.Services.VideoIndexer.Models;

public record ArmAccessTokenRequest
{
    [JsonPropertyName("permissionType")]
    public string PermissionType { get; init; } = string.Empty;

    [JsonPropertyName("scope")]
    public ArmAccessTokenScope Scope { get; init; }

    [JsonPropertyName("projectId")]
    public string ProjectId { get; init; } = string.Empty;

    [JsonPropertyName("videoId")]
    public string VideoId { get; init; } = string.Empty;
}