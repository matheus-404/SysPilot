using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using SysPilot.Models;

namespace SysPilot.Services
{

    public sealed class AppDownloadService
    {
        private readonly HttpClient _httpClient;
        private readonly HashSet<string> _downloadsInProgress = new();

        public AppDownloadService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add(
                "User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        }

        public async Task<DownloadResult> EnsureDownloadedAsync(AppItem item)
        {
            if (!_downloadsInProgress.Add(item.Name))
                return DownloadResult.AlreadyInProgress;

            try
            {
                var installDir = AppFileSystem.GetInstallDirectory(item);
                Directory.CreateDirectory(installDir);

                var existingExe = AppFileSystem.FindExecutable(installDir, item);
                if (existingExe is not null)
                    return DownloadResult.Success(existingExe);

                item.StatusText = "Downloading…";

                var extension = item.DownloadExtension.ToLowerInvariant();
                var downloadPath = Path.Combine(Path.GetTempPath(), $"{AppFileSystem.SanitizeFileName(item.Name)}{extension}");

                if (string.IsNullOrWhiteSpace(item.DownloadUrl))
                {
                    throw new InvalidOperationException($"Download URL for item '{item.Name}' cannot be null or empty.");
                }

                await DownloadToFileAsync(item.DownloadUrl, downloadPath);

                if (extension == ".exe")
                {
                    ValidateDownloadedExe(downloadPath);
                }

                if (extension == ".zip")
                {
                    ExtractZip(item, downloadPath, installDir);
                }
                else
                {
                    FinalizeNonZipInstaller(item, downloadPath, installDir, extension);
                }

                var exePath = AppFileSystem.FindExecutable(installDir, item);
                return exePath is not null ? DownloadResult.Success(exePath) : DownloadResult.NoExecutableFound;
            }
            catch (Exception ex)
            {
                throw new DownloadFailedException(ex);
            }
            finally
            {
                item.StatusText = "Launch";
                _downloadsInProgress.Remove(item.Name);
            }
        }

        private async Task DownloadToFileAsync(string url, string downloadPath)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Referer", url);

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            if ((int)response.StatusCode == 403 || !response.IsSuccessStatusCode)
            {
                throw new BotProtectionException("BotProtectionBlock");
            }

            await using var fileStream = new FileStream(downloadPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
            await response.Content.CopyToAsync(fileStream);
        }

        private static void ValidateDownloadedExe(string downloadPath)
        {
            using var fs = File.OpenRead(downloadPath);
            if (fs.Length < 2)
            {
                throw new InvalidDataException("Downloaded file is empty.");
            }

            Span<byte> header = stackalloc byte[2];
            fs.ReadExactly(header);
            if (header[0] != 'M' || header[1] != 'Z')
            {
                throw new BotProtectionException("BotProtectionBlock");
            }
        }

        private static void ExtractZip(AppItem item, string downloadPath, string installDir)
        {
            item.StatusText = "Extracting…";
            try
            {
                ZipFile.ExtractToDirectory(downloadPath, installDir, overwriteFiles: true);
            }
            catch (IOException)
            {
                throw new IOException($"Could not extract files. Make sure '{item.Name}' is closed before updating.");
            }
            finally
            {
                if (File.Exists(downloadPath)) File.Delete(downloadPath);
            }
        }

        private static void FinalizeNonZipInstaller(AppItem item, string downloadPath, string installDir, string extension)
        {
            item.StatusText = "Finalizing…";
            var finalInstallerPath = Path.Combine(installDir, $"{AppFileSystem.SanitizeFileName(item.Name)}{extension}");

            if (File.Exists(finalInstallerPath))
                File.Delete(finalInstallerPath);

            File.Move(downloadPath, finalInstallerPath);
        }
    }

    public readonly struct DownloadResult
    {
        public string? ExecutablePath { get; }
        public bool WasAlreadyInProgress { get; }
        public bool NoExecutableWasFound { get; }

        private DownloadResult(string? executablePath, bool wasAlreadyInProgress, bool noExecutableWasFound)
        {
            ExecutablePath = executablePath;
            WasAlreadyInProgress = wasAlreadyInProgress;
            NoExecutableWasFound = noExecutableWasFound;
        }

        public static DownloadResult Success(string executablePath) => new(executablePath, false, false);
        public static DownloadResult AlreadyInProgress { get; } = new(null, true, false);
        public static DownloadResult NoExecutableFound { get; } = new(null, false, true);
    }

    public sealed class DownloadFailedException : Exception
    {
        public DownloadFailedException(Exception inner)
            : base(inner.Message, inner)
        {
        }

        public bool IsBotProtection =>
            InnerException is BotProtectionException
            || InnerException is HttpRequestException
            || InnerException is InvalidDataException;
    }
}