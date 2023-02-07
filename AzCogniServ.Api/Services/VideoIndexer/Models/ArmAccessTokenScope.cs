using System.Text.Json.Serialization;

namespace AzCogniServ.Api.Services.VideoIndexer.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ArmAccessTokenScope
{
    Account,
    Video
}