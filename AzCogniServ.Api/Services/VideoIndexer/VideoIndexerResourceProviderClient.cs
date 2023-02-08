using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using AzCogniServ.Api.Services.VideoIndexer.Models;
using Azure.Core;
using Azure.Identity;

namespace AzCogniServ.Api.Services.VideoIndexer;

/*
 * Based on the official example: https://github.com/Azure-Samples/media-services-video-indexer/tree/master/ApiUsage/ArmBased
 */
public sealed class VideoIndexerResourceProviderClient
{
    private const string AzureResourceManagerHostname = "https://management.azure.com";
    private const string ApiHostname = "https://api.videoindexer.ai";
    private const string ApiVersion = "2022-08-01";
    private const string DefaultLang = "en-US";
    
    private readonly string accessToken;
    private readonly string subscriptionId;
    private readonly string resourceGroup;
    private readonly string accountName;
    private readonly ILogger<VideoIndexerResourceProviderClient> logger;

    private ArmAccount? account;
    private string? accountAccessToken;

    public VideoIndexerResourceProviderClient(string accessToken, string subscriptionId, string resourceGroup, string accountName,
        ILogger<VideoIndexerResourceProviderClient> logger)
    {
        this.accessToken = accessToken;
        this.subscriptionId = subscriptionId;
        this.resourceGroup = resourceGroup;
        this.accountName = accountName;
        this.logger = logger;
    }

    private static TokenRequestContext DefaultContext { get; } = new(new[] { $"{AzureResourceManagerHostname}/.default" });

    public static async Task<VideoIndexerResourceProviderClient> Of(string subscriptionId, string resourceGroup, string accountName,
        ILogger<VideoIndexerResourceProviderClient> logger)
    {
        var tokenRequestResult = await new DefaultAzureCredential().GetTokenAsync(DefaultContext);
        
        return new VideoIndexerResourceProviderClient(tokenRequestResult.Token, subscriptionId, resourceGroup, accountName, logger);
    }
    
    public async Task<string> GetVideoAccessToken(string videoId, CancellationToken cancellationToken = default)
    {
        var request = new ArmAccessTokenRequest
        {
            PermissionType = "Contributor",
            Scope = ArmAccessTokenScope.Video,
            VideoId = videoId
        };

        return await RequestAccessToken(request, cancellationToken);
    }

    /*
     * Supported media formats at: https://learn.microsoft.com/en-us/azure/media-services/latest/encode-media-encoder-standard-formats-reference
     */
    public async Task<string> UploadVideoFrom(Stream fileStream, string name, string description,
        bool isTrialAccount = false, CancellationToken cancellationToken = default)
    {
        await InitializeAccountAccessTokenIfNeeded(cancellationToken);
        await InitializeAccountDetailsIfNeeded(cancellationToken);
        
        var queryParams = QueryString.Create(
                new Dictionary<string, string>
                {
                    {"name", Path.GetFileNameWithoutExtension(name)},
                    {"description", description},
                    {"fileName", name},
                    {"privacy", "Private"},
                    {"language", DefaultLang},
                    {"accessToken", accountAccessToken!}
                }!);
        var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(fileStream);
        
        content.Add(streamContent, "file", name);
        streamContent.Headers.ContentDisposition!.FileNameStar = "";

        using var httpClient = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false });
        
        httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionId);
        httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
        
        var response = await httpClient.PostAsync(
            $"{ApiHostname}/{(isTrialAccount ? "trial" : account!.Location)}/Accounts/{account!.Properties.Id}/Videos{queryParams}",
            content,
            cancellationToken);
        
        if (response.StatusCode != HttpStatusCode.OK)
            throw new Exception($"Request failed with code [{response.StatusCode}] and reason: {response.ReasonPhrase}");
        
        var videoId = (await response.Content.ReadFromJsonAsync<ArmVideo>(cancellationToken: cancellationToken))?.Id
            ?? throw new Exception("No video ID was returned");
        
        return videoId;
    }

    public async Task WaitForCompletionOf(string videoId, bool isTrialAccount = false, CancellationToken cancellationToken = default)
    {
        await InitializeAccountAccessTokenIfNeeded(cancellationToken);
        await InitializeAccountDetailsIfNeeded(cancellationToken);
        
        var isAnalysisComplete = false;
        
        while (!isAnalysisComplete)
        {
            var queryParams = QueryString.Create(
                    new Dictionary<string, string>
                    {
                        {"accessToken", accountAccessToken!},
                        {"language", DefaultLang},
                    }!);

            using var httpClient = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false });
            
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionId);
            httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            
            var response = await httpClient.GetAsync(
                $"{ApiHostname}/{(isTrialAccount ? "trial" : account!.Location)}/Accounts/{account!.Properties.Id}/Videos/{videoId}/Index{queryParams}",
                cancellationToken);

            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception($"Request failed with code [{response.StatusCode}] and reason: {response.ReasonPhrase}");
            
            var processingState = (await response.Content.ReadFromJsonAsync<ArmVideo>(cancellationToken: cancellationToken))?.State;

            if (processingState == ArmProcessingState.Processed.ToString())
            {
                isAnalysisComplete = true;
                continue;
            }

            logger.LogDebug("Waiting for video [{VideoId}]", videoId);
            
            if (processingState == ArmProcessingState.Failed.ToString())
                throw new Exception($"Something went wrong: {await response.Content.ReadAsStringAsync(cancellationToken: cancellationToken)}");

            await Task.Delay(10000, cancellationToken);
        }
    }

    public async Task<string> GetVideoBy(string id, CancellationToken cancellationToken = default)
    {
        var videoAccessToken = await GetVideoAccessToken(id, cancellationToken);
        var queryParams = QueryString.Create(
            new Dictionary<string, string>
            {
                {"accessToken", videoAccessToken},
                {"id", id},
            }!);

        await InitializeAccountDetailsIfNeeded(cancellationToken);
        
        using var httpClient = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false });
        
        httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionId);
        httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
        
        var response = await httpClient.GetAsync(
            $"{ApiHostname}/{account!.Location}/Accounts/{account.Properties.Id}/Videos/Search{queryParams}",
            cancellationToken);
        
        if (response.StatusCode != HttpStatusCode.OK)
            throw new Exception($"Request failed with code [{response.StatusCode}] and reason: {response.ReasonPhrase}");
        
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
    
    private async Task<string> RequestAccessToken(ArmAccessTokenRequest request, CancellationToken cancellationToken = default)
    {
        var jsonRequestBody = JsonSerializer.Serialize(request);
        var httpContent = new StringContent(jsonRequestBody, System.Text.Encoding.UTF8, "application/json");
        using var client = new HttpClient();
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.PostAsync(
            $"{AzureResourceManagerHostname}/subscriptions/{subscriptionId}/resourcegroups/{resourceGroup}/providers/Microsoft.VideoIndexer/accounts/{accountName}/generateAccessToken?api-version={ApiVersion}",
            httpContent,
            cancellationToken);

        if (response.StatusCode != HttpStatusCode.OK)
            throw new Exception($"Request failed with code [{response.StatusCode}] and reason: {response.ReasonPhrase}");
        
        var jsonResponseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        
        return JsonSerializer.Deserialize<ArmAccessTokenResponse>(jsonResponseBody)?.AccessToken
            ?? throw new Exception("No access token was returned");
    }

    private async Task InitializeAccountDetailsIfNeeded(CancellationToken cancellationToken = default)
    {
        if (account is not null)
            return;
        
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.GetAsync(
            $"{AzureResourceManagerHostname}/subscriptions/{subscriptionId}/resourcegroups/{resourceGroup}/providers/Microsoft.VideoIndexer/accounts/{accountName}?api-version={ApiVersion}",
            cancellationToken);

        if (response.StatusCode != HttpStatusCode.OK)
            throw new Exception($"Request failed with code [{response.StatusCode}] and reason: {response.ReasonPhrase}");
            
        account = await response.Content.ReadFromJsonAsync<ArmAccount>(cancellationToken: cancellationToken);
        
        if (account is null || string.IsNullOrWhiteSpace(account.Location) || account.Properties == null || string.IsNullOrWhiteSpace(account.Properties.Id))
            throw new Exception($"Account [{accountName}] not found.");
    }

    private async Task InitializeAccountAccessTokenIfNeeded(CancellationToken cancellationToken = default)
    {
        if (accountAccessToken is not null)
            return;

        var request = new ArmAccessTokenRequest
        {
            PermissionType = "Contributor",
            Scope = ArmAccessTokenScope.Account
        };

        accountAccessToken = await RequestAccessToken(request, cancellationToken);
    }
}