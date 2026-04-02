namespace L2Launcher.Models;

public sealed class ManifestFile
{
    public required string Path { get; init; }
    public required string Hash { get; init; }
    public long Size { get; init; }
    public bool IsCritical { get; init; }

    public string CompressedRemotePath => $"{Path}.bz2";
}