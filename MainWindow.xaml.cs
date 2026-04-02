using System.IO;
using System.Net;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using L2Launcher.Models;
using L2Launcher.Services;

namespace L2Launcher;

public partial class MainWindow : Window
{
    private const string BaseUpdaterUrl = "https://your-website.com/updater/";
    private const string ManifestUrl = BaseUpdaterUrl + "files.xml";
    private const string DownloadUrl = BaseUpdaterUrl + "download.php";

    private const string WebsiteUrl  = "https://your-website.com";
    private const string DiscordUrl  = "https://discord.gg/yourinvite";
    private const string FacebookUrl = "https://facebook.com/yourpage";

    private const string GameExeRelativePath = @"system\L2.exe";

    private const double ProgressTrackWidth = 815.0;

    private readonly HttpClient _httpClient;
    private readonly ManifestService _manifestService;
    private readonly HashService _hashService;
    private readonly DownloadService _downloadService;
    private readonly LauncherService _launcherService;

    private bool _isBusy;

    public MainWindow()
    {
        InitializeComponent();

        _httpClient = new HttpClient(new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.All
        })
        {
            Timeout = TimeSpan.FromMinutes(30)
        };

        _manifestService = new ManifestService(_httpClient);
        _hashService = new HashService();
        _downloadService = new DownloadService(_httpClient);
        _launcherService = new LauncherService();

        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await RunFullCheckAsync();
    }

    private async Task RunCriticalCheckAsync()
    {
        if (_isBusy)
            return;

        try
        {
            SetBusy(true, "Checking critical files...");

            var manifest = await _manifestService.LoadManifestAsync(ManifestUrl);
            var criticalFiles = manifest.Where(x => x.IsCritical).ToList();
            var filesToDownload = GetOutdatedFiles(criticalFiles);

            if (filesToDownload.Count == 0)
            {
                SetReady("Ready to start");
                return;
            }

            await DownloadFilesAsync(filesToDownload, "Downloading critical files");
            SetReady("Ready to start");
        }
        catch (Exception ex)
        {
            SetError($"Critical check failed: {ex.Message}");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task RunFullCheckAsync()
    {
        if (_isBusy)
            return;

        try
        {
            SetBusy(true, "Checking files...");

            var manifest = await _manifestService.LoadManifestAsync(ManifestUrl);
            var filesToDownload = GetOutdatedFiles(manifest);

            if (filesToDownload.Count == 0)
            {
                SetReady("Client is up to date");
                return;
            }

            await DownloadFilesAsync(filesToDownload, "Downloading files");
            SetReady("Ready to start");
        }
        catch (Exception ex)
        {
            SetError($"Full check failed: {ex.Message}");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private List<ManifestFile> GetOutdatedFiles(IEnumerable<ManifestFile> manifestFiles)
    {
        var root = AppContext.BaseDirectory;
        var filesToDownload = new List<ManifestFile>();

        foreach (var file in manifestFiles)
        {
            var localPath = Path.Combine(root, file.Path);

            if (!_hashService.IsFileValid(localPath, file.Hash))
                filesToDownload.Add(file);
        }

        return filesToDownload;
    }

    private async Task DownloadFilesAsync(List<ManifestFile> filesToDownload, string statusPrefix)
    {
        long totalFiles = filesToDownload.Count;
        long currentFileIndex = 0;

        SetProgressPercent(0);

        foreach (var file in filesToDownload)
        {
            currentFileIndex++;

            ProgressText.Text = $"[{currentFileIndex}/{totalFiles}] {Path.GetFileName(file.Path)}";

            var localDestination = Path.Combine(AppContext.BaseDirectory, file.Path);
            var remoteCompressedPath = file.CompressedRemotePath.Replace('\\', '/');

            var progress = new Progress<double>(writtenBytes =>
            {
                var basePercent = ((double)(currentFileIndex - 1) / totalFiles) * 100.0;
                var fileWeight = 100.0 / totalFiles;
                var approxFilePercent = file.Size > 0
                    ? Math.Min(1.0, writtenBytes / file.Size)
                    : 0.5;

                var totalPercent = Math.Min(100.0, basePercent + (fileWeight * approxFilePercent));
                SetProgressPercent(totalPercent);
            });

            await _downloadService.DownloadAndExtractSingleFileAsync(
                DownloadUrl,
                remoteCompressedPath,
                localDestination,
                progress);

            SetProgressPercent((double)currentFileIndex / totalFiles * 100.0);
        }

        ProgressText.Text = "Ready to start";
    }

    private void SetBusy(bool busy, string? status = null)
    {
        _isBusy = busy;

        FullCheckButton.IsEnabled = !busy;
        GameStartButton.IsEnabled = !busy && IsGameExecutablePresent();

        if (!string.IsNullOrWhiteSpace(status))
            ProgressText.Text = status;
    }

    private void SetReady(string status)
    {
        ProgressText.Text = status;
        SetProgressPercent(100);
        GameStartButton.IsEnabled = IsGameExecutablePresent();
    }

    private void SetError(string status)
    {
        ProgressText.Text = status;
        GameStartButton.IsEnabled = false;
    }

    private void SetProgressPercent(double percent)
    {
        percent = Math.Max(0, Math.Min(100, percent));
        ProgressFill.Width = ProgressTrackWidth * (percent / 100.0);
    }

    private bool IsGameExecutablePresent()
    {
        var fullPath = Path.Combine(AppContext.BaseDirectory, GameExeRelativePath);
        return File.Exists(fullPath);
    }

    private async void FullCheckButton_Click(object sender, RoutedEventArgs e)
    {
        await RunFullCheckAsync();
    }

    private void GameStartButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var fullPath = Path.Combine(AppContext.BaseDirectory, GameExeRelativePath);
            _launcherService.StartGame(fullPath);
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Launch Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void HomeButton_Click(object sender, RoutedEventArgs e)
    {
        _launcherService.OpenUrl(WebsiteUrl);
    }

    private void DiscordButton_Click(object sender, RoutedEventArgs e)
    {
        _launcherService.OpenUrl(DiscordUrl);
    }

    private void FacebookButton_Click(object sender, RoutedEventArgs e)
    {
        _launcherService.OpenUrl(FacebookUrl);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
            DragMove();
    }
}