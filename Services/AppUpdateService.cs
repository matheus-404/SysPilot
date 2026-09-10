using System;
using System.Threading.Tasks;
using Velopack;
using Velopack.Sources;

namespace SysPilot.Services
{
    public sealed class AppUpdateService
    {
        private const string GitHubRepoUrl = "https://github.com/matheus-404/SysPilot";

        private readonly UpdateManager _updateManager;

        public AppUpdateService()
        {
            _updateManager = new UpdateManager(
                new GithubSource(GitHubRepoUrl, accessToken: null, prerelease: false));
        }

        public bool IsInstalled => _updateManager.IsInstalled;

        public string? CurrentVersion =>
            IsInstalled ? _updateManager.CurrentVersion?.ToString() : null;

        // Checks for, downloads, and applies an update, then restarts the app.
        // Returns false if there is no update or the app isn't installed (e.g. running from IDE).
        public async Task<bool> CheckDownloadAndApplyAsync(IProgress<int>? progress = null)
        {
            if (!IsInstalled)
                return false;

            var newVersion = await _updateManager.CheckForUpdatesAsync();
            if (newVersion is null)
                return false;

            Action<int>? progressCallback = progress is null ? null : progress.Report;
            await _updateManager.DownloadUpdatesAsync(newVersion, progressCallback);

            // Applies the update and restarts the app. This does not return.
            _updateManager.ApplyUpdatesAndRestart(newVersion);
            return true;
        }
    }
}
