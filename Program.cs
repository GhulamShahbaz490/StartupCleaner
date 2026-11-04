using System;
using System.IO;
using System.Linq;

class Program
{
    static void Main()
    {
        var dirs = new[]
        {
            Environment.GetEnvironmentVariable("TEMP"),
            Environment.GetEnvironmentVariable("TMP"),
            @"C:\Windows\Temp",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp"),
            // Common browser caches
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Google\Chrome\User Data\Default\Cache"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Edge\User Data\Default\Cache"),
            // Windows caches (Temporary Internet Files, WER, DirectX Shader Cache)
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Windows\INetCache"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Windows\WER"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Windows\D3DSCache"),
        };

        foreach (var d in dirs)
        {
            if (string.IsNullOrWhiteSpace(d) || !Directory.Exists(d)) continue;
            CleanDirectory(d, 0);
        }

        // Thumbnail cache: delete the thumbcache_*.db files only, never the Explorer folder,
        // and never unlock (unlocking would kill explorer.exe).
        CleanFiles(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Windows\Explorer"), "thumbcache_*.db", allowUnlock: false);

        // Delivery Optimization cache (elevated, best effort)
        try { Run("powershell", "-NoProfile -Command \"Delete-DeliveryOptimizationCache -Force\""); } catch { }

        // Admin-required cache contents (root folders are kept; Windows recreates them)
        var protectedContentDirs = new[]
        {
            @"C:\ProgramData\Microsoft\Windows\WER",
            @"C:\Windows\ServiceProfiles\NetworkService\AppData\Local\Microsoft\Windows\DeliveryOptimization\Cache",
        };
        foreach (var d in protectedContentDirs)
        {
            if (Directory.Exists(d)) CleanDirectory(d, 0);
        }

        // Admin-required cleanup targets (executed when elevated via scheduled task)
        var protectedTargets = new[]
        {
            @"C:\.GamingRoot",
            @"C:\DumpStack.log.tmp",
            @"C:\Program Files\Paseo",
            @"C:\Windows\Temp",
        };
        foreach (var p in protectedTargets)
        {
            TryDeleteProtected(p);
        }

        // DNS cache flush (best effort)
        try { Run("ipconfig", "/flushdns"); } catch { }
    }

    static void CleanFiles(string dir, string pattern, bool allowUnlock)
    {
        if (!Directory.Exists(dir)) return;
        try
        {
            foreach (var file in Directory.EnumerateFiles(dir, pattern, SearchOption.TopDirectoryOnly))
            {
                try { File.Delete(file); }
                catch
                {
                    if (!allowUnlock) continue;
                    TryUnlock(file);
                    try { File.Delete(file); } catch { }
                }
            }
        }
        catch { }
    }

    static void CleanDirectory(string path, int depth)
    {
        if (depth > 3) return;
        try
        {
            foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.TopDirectoryOnly))
            {
                try { File.Delete(file); } catch
                {
                    TryUnlock(file);
                    try { File.Delete(file); } catch { }
                }
            }
            foreach (var sub in Directory.EnumerateDirectories(path))
            {
                try
                {
                    // Delete subdirectory contents then the dir itself when possible
                    CleanDirectory(sub, depth + 1);
                    Directory.Delete(sub, true);
                }
                catch { /* skip locked/system dirs */ }
            }
        }
        catch { }
    }

    static void TryUnlock(string path)
    {
        string[] candidates = new[]
        {
            @"C:\Users\SHAHBAZ\AppData\Local\PowerToys\bin\PowerToys.FileLocksmith.CLI.exe",
            @"C:\Program Files\PowerToys\PowerToys.FileLocksmith.CLI.exe",
        };
        string cli = candidates.FirstOrDefault(File.Exists);
        if (cli == null) return;
        try
        {
            var p = new System.Diagnostics.Process();
            p.StartInfo.FileName = cli;
            p.StartInfo.Arguments = $"--kill \"{path}\"";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            p.WaitForExit(10000);
        }
        catch { }
    }

    static void TryDeleteProtected(string path)
    {
        if (!File.Exists(path) && !Directory.Exists(path)) return;
        try { File.SetAttributes(path, FileAttributes.Normal); } catch { }
        TryUnlock(path);
        try
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
            else
                File.Delete(path);
        }
        catch { }
    }

    static void Run(string cmd, string args)
    {
        var p = new System.Diagnostics.Process();
        p.StartInfo.FileName = cmd;
        p.StartInfo.Arguments = args;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.CreateNoWindow = true;
        p.Start();
        p.WaitForExit(5000);
    }
}