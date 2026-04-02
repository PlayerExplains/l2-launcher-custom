using System.Diagnostics;
using System.IO;

namespace L2Launcher.Services;

public sealed class LauncherService
{
    public void OpenUrl(string url)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }

    public void StartGame(string gameExeFullPath)
    {
        if (!File.Exists(gameExeFullPath))
            throw new FileNotFoundException("Game executable not found.", gameExeFullPath);

        var workingDir = Path.GetDirectoryName(gameExeFullPath) ?? AppContext.BaseDirectory;

        Process.Start(new ProcessStartInfo
        {
            FileName = gameExeFullPath,
            WorkingDirectory = workingDir,
            UseShellExecute = true
        });
    }
}