using System.IO;
using System.Net.Http;
using ICSharpCode.SharpZipLib.BZip2;

namespace L2Launcher.Services;

public sealed class DownloadService
{
    private readonly HttpClient _httpClient;

    public DownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task DownloadAndExtractSingleFileAsync(
        string downloadUrl,
        string remoteCompressedPath,
        string destinationFullPath,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["files"] = remoteCompressedPath
        });

        using var response = await _httpClient.PostAsync(downloadUrl, form, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);

        var destinationDirectory = Path.GetDirectoryName(destinationFullPath);
        if (!string.IsNullOrWhiteSpace(destinationDirectory))
            Directory.CreateDirectory(destinationDirectory);

        await using var outputFile = File.Create(destinationFullPath);

        using var bufferedInput = new MemoryStream();
        await responseStream.CopyToAsync(bufferedInput, cancellationToken);
        bufferedInput.Position = 0;

        BZip2.Decompress(bufferedInput, outputFile, false);

        if (outputFile.CanSeek)
            progress?.Report(outputFile.Length);
    }
}