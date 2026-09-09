using System;
using System.IO;
using System.Linq;
using System.Text;
using SysPilot.Models;

namespace SysPilot.Services
{
    public static class AppFileSystem
    {
        public const string DefenderRemoverAppName = "Defender Remover";

        public const string DduAppName = "DDU";

        private const string DefenderRemoverScriptName = "Script_Run.cmd";

        private static readonly char[] s_invalidChars = Path.GetInvalidFileNameChars();

        private static readonly string[] s_preferredExeNames =
        {
            "Display Driver Uninstaller.exe", "procexp64.exe", "procexp.exe",
            "Autoruns64.exe", "BCUninstaller.exe", "NVCleanstall.exe"
        };

        public static string GetInstallDirectory(AppItem item)
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(localAppData, "SysPilot", SanitizeFileName(item.Name));
        }

        public static string GetDefaultInstallerPath(string installDir, AppItem item)
        {
            var extension = item.DownloadExtension.ToLowerInvariant();
            return Path.Combine(installDir, $"{SanitizeFileName(item.Name)}{extension}");
        }

        public static string SanitizeFileName(string name)
        {
            var sb = new StringBuilder(name.Length);
            foreach (var c in name)
            {
                if (!char.IsWhiteSpace(c) && Array.IndexOf(s_invalidChars, c) < 0)
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
        public static bool IsValidExe(string path)
        {
            try
            {
                if (!File.Exists(path)) return false;
                using var fs = File.OpenRead(path);
                if (fs.Length < 2) return false;

                Span<byte> header = stackalloc byte[2];
                fs.ReadExactly(header);
                return header[0] == 'M' && header[1] == 'Z';
            }
            catch
            {
                return false;
            }
        }

        public static string? FindExecutable(string installDir, AppItem item)
        {
            if (!Directory.Exists(installDir))
                return null;

            if (item.Name.Equals(DefenderRemoverAppName, StringComparison.OrdinalIgnoreCase))
            {
                var candidates = Directory.GetFiles(installDir, DefenderRemoverScriptName, SearchOption.AllDirectories);
                if (candidates.Length > 0)
                    return candidates[0];
            }

            foreach (var name in s_preferredExeNames)
            {
                var candidates = Directory.GetFiles(installDir, name, SearchOption.AllDirectories);
                foreach (var candidate in candidates)
                {
                    if (IsValidExe(candidate))
                        return candidate;

                    TryDelete(candidate);
                }
            }

            var extension = item.DownloadExtension.ToLowerInvariant();

            if (extension == ".zip")
            {
                return Directory.EnumerateFiles(installDir, "*.exe", SearchOption.TopDirectoryOnly)
                    .FirstOrDefault(IsValidExe);
            }

            var expectedFileName = $"{SanitizeFileName(item.Name)}{extension}";
            var candidatePath = Path.Combine(installDir, expectedFileName);

            if (IsValidExe(candidatePath))
                return candidatePath;

            return Directory.EnumerateFiles(installDir, $"*{extension}", SearchOption.TopDirectoryOnly)
                .FirstOrDefault(IsValidExe);
        }

        private static void TryDelete(string path)
        {
            try { File.Delete(path); } catch { /* best-effort cleanup, ignore */ }
        }
    }
}
