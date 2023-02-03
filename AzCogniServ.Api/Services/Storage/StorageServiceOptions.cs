namespace AzCogniServ.Api.Services.Storage;

public sealed record StorageServiceOptions
{
    public const string ConfigKey = "Storage";

    public string ContainerName { get; init; } = string.Empty;
}