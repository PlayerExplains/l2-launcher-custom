using System.IO;
using System.Security.Cryptography;

namespace L2Launcher.Services;

public sealed class HashService
{
    public string ComputeMd5(string filePath)
    {
        using var md5 = MD5.Create();
        using var stream = File.OpenRead(filePath);
        var hash = md5.ComputeHash(stream);
        return Convert.ToHexString(hash);
    }

    public bool IsFileValid(string fullPath, string expectedHash)
    {
        if (!File.Exists(fullPath))
            return false;

        var actual = ComputeMd5(fullPath);
        return string.Equals(actual, expectedHash, StringComparison.OrdinalIgnoreCase);
    }
}