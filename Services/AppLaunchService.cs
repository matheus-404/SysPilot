using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Threading;
using SysPilot.Models;

namespace SysPilot.Services
{

    public sealed class AppLaunchService
    {
        private const int Win32ErrorUacDeclined = 1223;

        public event Func<AppItem, Exception, Task>? LaunchFailed;

        public bool IsRawDduInstaller(AppItem item, string installDir)
        {
            if (!item.Name.Equals(AppFileSystem.DduAppName, StringComparison.OrdinalIgnoreCase))
                return false;

            var rawInstallerPath = AppFileSystem.GetDefaultInstallerPath(installDir, item);
            return item.ExecutablePath.Equals(rawInstallerPath, StringComparison.OrdinalIgnoreCase);
        }

        public async Task LaunchAsync(AppItem item, string installDir)
        {
            bool isRawInstaller = IsRawDduInstaller(item, installDir);

            try
            {
                var process = StartProcess(item.ExecutablePath, item.Arguments);

                if (isRawInstaller && process is not null)
                {
                    item.StatusText = "Extracting…";
                    _ = WaitForDduExtractionAndRelaunchAsync(item, installDir, process);
                    return;
                }
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == Win32ErrorUacDeclined)
            {
                // UAC declined
            }
            catch (Exception ex)
            {
                await RaiseLaunchFailedAsync(item, ex);
            }
            finally
            {
                if (!isRawInstaller)
                {
                    item.StatusText = "Launch";
                }
            }
        }

        private static Process? StartProcess(string executablePath, string arguments)
        {
            var psi = new ProcessStartInfo
            {
                FileName = executablePath,
                Arguments = arguments,
                UseShellExecute = true,
                WorkingDirectory = Path.GetDirectoryName(executablePath) ?? string.Empty
            };

            return Process.Start(psi);
        }

        private async Task WaitForDduExtractionAndRelaunchAsync(AppItem item, string installDir, Process installerProcess)
        {
            var rawInstallerPath = AppFileSystem.GetDefaultInstallerPath(installDir, item);

            try
            {
                await installerProcess.WaitForExitAsync();

                string? extractedExe = null;
                for (int i = 0; i < 30; i++)
                {
                    extractedExe = AppFileSystem.FindExecutable(installDir, item);
                    if (IsNewlyExtracted(extractedExe, rawInstallerPath))
                    {
                        break;
                    }
                    await Task.Delay(1000);
                }

                if (IsNewlyExtracted(extractedExe, rawInstallerPath))
                {
                    Dispatcher.UIThread.Post(async () =>
                    {
                        item.ExecutablePath = extractedExe!;
                        item.StatusText = "Launch";

                        try
                        {
                            StartProcess(extractedExe!, item.Arguments);
                        }
                        catch (Win32Exception winEx) when (winEx.NativeErrorCode == Win32ErrorUacDeclined)
                        {
                            // UAC declined
                        }
                        catch (Exception ex)
                        {
                            await RaiseLaunchFailedAsync(item, ex);
                        }
                    });
                }
                else
                {
                    Dispatcher.UIThread.Post(() => { item.StatusText = "Launch"; });
                }
            }
            catch
            {
                Dispatcher.UIThread.Post(() => { item.StatusText = "Launch"; });
            }
        }

        private static bool IsNewlyExtracted(string? candidatePath, string rawInstallerPath) =>
            !string.IsNullOrWhiteSpace(candidatePath)
            && !candidatePath.Equals(rawInstallerPath, StringComparison.OrdinalIgnoreCase);

        private async Task RaiseLaunchFailedAsync(AppItem item, Exception ex)
        {
            if (LaunchFailed is not null)
            {
                await LaunchFailed.Invoke(item, ex);
            }
        }
    }
}
