using Azure;
using Azure.Storage.Blobs.Models;

namespace AzCogniServ.Api.Services.Storage;

public sealed class AsyncResourceEnumerator : IAsyncEnumerable<string>
{
    private readonly AsyncPageable<BlobItem> items;

    public AsyncResourceEnumerator(AsyncPageable<BlobItem> items)
    {
        this.items = items;
    }

    public async IAsyncEnumerator<string> GetAsyncEnumerator(CancellationToken cancellationToken = new())
    {
        await foreach (var page in items.AsPages().WithCancellation(cancellationToken))
        {
            foreach (var value in page.Values)
            {
                if (!Path.GetFileNameWithoutExtension(value.Name).EndsWith(".meta"))
                    yield return value.Name;
            }
        }
    }
}