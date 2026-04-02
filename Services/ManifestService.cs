using System.Net.Http;
using System.Xml.Linq;
using L2Launcher.Models;

namespace L2Launcher.Services;

public sealed class ManifestService
{
    private readonly HttpClient _httpClient;

    public ManifestService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<ManifestFile>> LoadManifestAsync(string manifestUrl, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(manifestUrl, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        response.EnsureSuccessStatusCode();

        XDocument doc;
        try
        {
            doc = XDocument.Parse(content);
        }
        catch (Exception ex)
        {
            var preview = content.Length > 500 ? content[..500] : content;
            throw new InvalidOperationException(
                $"Manifest is not valid XML. URL: {manifestUrl}\n\nServer response starts with:\n{preview}",
                ex);
        }

        var result = new List<ManifestFile>();

        foreach (var node in doc.Root?.Elements() ?? Enumerable.Empty<XElement>())
        {
            var nodeName = node.Name.LocalName.ToLowerInvariant();
            if (nodeName is not ("critical" or "normal"))
                continue;

            var path = node.Attribute("path")?.Value?.Trim();
            var hash = node.Attribute("hash")?.Value?.Trim();
            var sizeValue = node.Attribute("size")?.Value?.Trim();

            if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(hash))
                continue;

            long.TryParse(sizeValue, out var size);

            result.Add(new ManifestFile
            {
                Path = path.Replace('/', '\\'),
                Hash = hash.ToUpperInvariant(),
                Size = size,
                IsCritical = nodeName == "critical"
            });
        }

        return result;
    }
}