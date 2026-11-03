# StartupCleaner

StartupCleaner is a Windows startup cleaner written in .NET. On every login it removes temporary and cache files from common locations, including user temp folders, `C:\Windows\Temp`, and browser caches. When run with administrator privileges it can also remove protected files using PowerToys File Locksmith to unlock locked files.

## About

- **Type:** Windows desktop console/WinExe utility
- **Framework:** .NET 10 / Windows x64
- **License:** MIT (recommended)
- **Author:** Shahbaz

## Features

- Cleans `%TEMP%`, `%TMP%`, `C:\Windows\Temp`, and local user temp folders
- Cleans Chrome and Edge cache folders
- Uses PowerToys File Locksmith CLI to unlock locked files before retrying deletion
- Requests administrator privileges for protected cleanup
- Cleans protected targets such as:
  - `C:\.GamingRoot`
  - `C:\DumpStack.log.tmp`
  - `C:\Program Files\Paseo`
- Runs automatically from the Windows Startup folder
- Self-contained single-file build for easy distribution

## Project Structure

```text
StartupCleaner/
├── Program.cs
├── StartupCleaner.csproj
├── app.manifest
├── README.md
└── .gitignore
```

## Build

```powershell
cd C:\Users\SHAHBAZ\StartupCleaner
dotnet build -c Release
```

## Publish Self-Contained Executable

```powershell
cd C:\Users\SHAHBAZ\StartupCleaner
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

The published executable will be available at:

```text
bin\Release\net10.0\win-x64\publish\StartupCleaner.exe
```

## Usage

1. Build and publish the executable.
2. Copy `StartupCleaner.exe` to your Startup folder:

```powershell
$startup = "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\Startup"
Copy-Item "bin\Release\net10.0\win-x64\publish\StartupCleaner.exe" $startup -Force
```

3. The app runs automatically on Windows login.
4. Because it requests administrator privileges, Windows will show a UAC prompt at startup.

## GitHub Repository Commands

Create a public repository on GitHub named `StartupCleaner`, then run:

```powershell
cd C:\Users\SHAHBAZ\StartupCleaner
git init
git add .
git commit -m "Initial release: Windows startup temp and cache cleaner"
git branch -M main
git remote add origin https://github.com/YOUR_USERNAME/StartupCleaner.git
git push -u origin main
```

## GitHub About Description

Use this for the repository About section:

```text
Windows startup cleaner in .NET that removes temp, cache, and protected leftover files. Uses PowerToys File Locksmith to unlock locked files and runs with administrator privileges.
```

## Suggested GitHub Topics

```text
dotnet csharp windows cleanup temp-cache file-locksmith powertoys startup-tool
```

## Deployment / Distribution Options

For a Windows desktop utility, recommended deployment is GitHub Releases:

1. Create a GitHub Release.
2. Upload `StartupCleaner.exe` as a release asset.
3. Share the release URL with users.

For documentation or a project page, free options include:

- **GitHub Pages** for static documentation
- **Render** free web services for ASP.NET Core web dashboards
- **Fly.io** free allowance for small .NET web apps
- **Azure App Service Free (F1)** for small ASP.NET Core apps

This project itself is a Windows desktop executable, so the primary deployment method should be GitHub Releases rather than a web app hosting platform.