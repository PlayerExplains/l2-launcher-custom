# L2 Launcher

Lineage 2 custom launcher with automatic updates, file verification, full client check, and quick access to community links.

## Video Guide

[![](https://img.youtube.com/vi/vmt1NkJWS-Y/maxresdefault.jpg)](https://www.youtube.com/watch?v=vmt1NkJWS-Y)

## Purpose

This project provides a custom launcher with:

- automatic client file checking
- automatic patch/update download
- manual **Full Check**
- **Start** button to launch `system\L2.exe`
- quick links for website, Discord, and Facebook
- custom fantasy-themed launcher UI

The launcher reads a remote manifest, compares local files by hash, downloads missing or outdated files, and then enables the game start button.

## URLs and values you need to change

Open `MainWindow.xaml.cs` and update these constants:

```csharp
private const string BaseUpdaterUrl = "https://your-website.com/updater/";

private const string WebsiteUrl = "https://your-website.com";
private const string DiscordUrl = "https://discord.gg/yourinvite";
private const string FacebookUrl = "https://facebook.com/yourpage";
```

You may also want to change the game executable path if needed:
```csharp
private const string GameExeRelativePath = @"system\L2.exe";
```

## Requirements
* Windows
* .NET SDK 8.0 or newer
 
**Install .NET SDK**

Download and install the .NET 8 SDK from Microsoft.

After installation, verify it:

```csharp
dotnet --version
```

**Install project dependencies**

From the project folder:

```csharp
dotnet restore
```

If needed, install SharpZipLib manually:

```csharp
dotnet add package SharpZipLib --version 1.4.2
```

**Build the project**
```csharp
dotnet build
```

**Run the launcher**

```csharp
dotnet run
```

**Build a single EXE**

Use this command to publish everything into one executable:

```csharp
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true
```

Published output will be created in:

bin\Release\net8.0-windows\win-x64\publish\

## How to set up the files for download

Run `PatchBuilder.exe` and select the source directory containing your Lineage 2 client files, then select the output directory where the patch package should be generated.

After clicking **NEXT**, review the scanned files:
- move any files you want treated as mandatory startup files into the **Critical Files** list
- leave the rest in **Normal Files**
- remove any files you do not want included in the patch package

When ready, click **BUILD!**.

The builder will generate:
- all selected files compressed as `.bz2`, preserving folder structure
- `files.xml`
- `download.php`

Upload the generated output folder contents to your web server inside the updater directory, for example:

```text
/updater/
    download.php
    files.xml
    system/
    textures/
    maps/
    ...