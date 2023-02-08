using Azure;
using Azure.Storage.Blobs.Models;

namespace AzCogniServ.Api.Services.Storage;

public sealed class AsyncResourceEnumerator : IAsyncEnumerable<string>
{
    private readonly AsyncPageable<BlobItem> items;
    private string[]? fileExtensions;

    public AsyncResourceEnumerator(AsyncPageable<BlobItem> items)
    {
        this.items = items;
    }

    public AsyncResourceEnumerator WithFileExtensions(params string[] extensions)
    {
        fileExtensions = extensions;
        return this;
    }

    public async IAsyncEnumerator<string> GetAsyncEnumerator(CancellationToken cancellationToken = new())
    {
        await foreach (var page in items.AsPages().WithCancellation(cancellationToken))
        {
            foreach (var value in page.Values)
            {
                if (!Path.GetFileNameWithoutExtension(value.Name).EndsWith(".meta") &&
                    (fileExtensions?.Contains(Path.GetExtension(value.Name)) ?? true))
                    yield return value.Name;
            }
        }
    }
}