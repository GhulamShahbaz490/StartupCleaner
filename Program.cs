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
        };

        foreach (var d in dirs)
        {
            if (string.IsNullOrWhiteSpace(d) || !Directory.Exists(d)) continue;
            CleanDirectory(d, 0);
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