using Avalonia.Controls;
using System;
using System.Threading;
using System.Threading.Tasks;
using Velopack;
using Velopack.Sources;

namespace SysPilot.Services
{
    public sealed class AppUpdateService
    {
        private const string GitHubRepoUrl = "https://github.com/matheus-404/SysPilot";

        private readonly UpdateManager? _updateManager;
        private readonly SemaphoreSlim _updateLock = new(1, 1);

        public AppUpdateService()
        {
            if (Design.IsDesignMode)
            {
                return;
            }

            _updateManager = new UpdateManager(
                new GithubSource(GitHubRepoUrl, accessToken: null, prerelease: false));
        }

        public bool IsInstalled => _updateManager?.IsInstalled ?? false;

        public string? CurrentVersion =>
            IsInstalled ? _updateManager?.CurrentVersion?.ToString() : null;

        // Checks for, downloads, and applies an update, then restarts the app.
        // Returns false if there is no update or the app isn't installed (e.g. running from IDE).
        // Safe to call concurrently — only one check/download/apply runs at a time;
        // a second concurrent call simply waits for the first to finish.
        public async Task<bool> CheckDownloadAndApplyAsync(IProgress<int>? progress = null)
        {
            if (!IsInstalled || _updateManager is null)
                return false;

            await _updateLock.WaitAsync();
            try
            {
                UpdateInfo? newVersion = await _updateManager.CheckForUpdatesAsync();
                if (newVersion is null)
                    return false;

                UpdateInfo confirmedVersion = newVersion;

                Action<int>? progressCallback = progress is null ? null : progress.Report;
                await _updateManager.DownloadUpdatesAsync(confirmedVersion, progressCallback);

                // Applies the update and restarts the app. This does not return.
                _updateManager.ApplyUpdatesAndRestart(confirmedVersion);
                return true;
            }
            finally
            {
                _updateLock.Release();
            }
        }
    }
}