using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;

async Task Scan(string dir, BlobContainerClient containerClient)
{
    var files = Directory.GetFiles(dir)
        .Where(f => f.EndsWith(".png") || f.EndsWith(".jpg") || f.EndsWith(".gif") || f.EndsWith(".webp") || f.EndsWith(".bmp"));

    foreach (var file in files)
    {
        Console.WriteLine($"Uploading [{file}]...");

        try
        {
            await using var stream = File.OpenRead(file);
            
            await containerClient.UploadBlobAsync(Path.GetFileName(file), await BinaryData.FromStreamAsync(stream));
        }
        catch (RequestFailedException e)
        {
            if (e.ErrorCode != "BlobAlreadyExists")
                throw;

            var newName = $"{Path.GetFileNameWithoutExtension(file)}.{Guid.NewGuid():N}{Path.GetExtension(file)}";
            
            Console.WriteLine($"Renamed duplicated file to [{newName}]");
            
            await using var stream = File.OpenRead(file);
            
            await containerClient.UploadBlobAsync(newName, await BinaryData.FromStreamAsync(stream));
            continue;
        }

        Console.WriteLine("Uploaded successfully");
    }

    var folders = Directory.GetDirectories(dir);

    foreach (var folder in folders)
        await Scan(Path.Combine(dir, folder), containerClient);
}

var dir = args.FirstOrDefault(".");

Console.WriteLine($"Scanning directory [{dir}]...");

var containerClient = new BlobContainerClient(
    new Uri("http://127.0.0.1:10000/devstoreaccount1/activities"),
    new StorageSharedKeyCredential("devstoreaccount1", "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw=="),
    new BlobClientOptions());

await Scan(dir, containerClient);