using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using SysPilot.Converters;
using SysPilot.Models;
using SysPilot.Services;

namespace SysPilot
{
    public partial class MainWindow : Window
    {
        private List<AppCategory> _categories = new();

        private readonly AppUpdateService _updateService = new();
        private readonly AppDownloadService _downloadService = new();
        private readonly AppLaunchService _launchService;
        private readonly CatalogService _catalogService = new();

        private readonly IImage _maximizeIconAsset;
        private readonly IImage _restoreIconAsset;

        public MainWindow()
        {
            InitializeComponent();

            _launchService = new AppLaunchService();

            _launchService.LaunchFailed +=
                (item, ex) => ShowMessageAsync(
                    $"Couldn't launch \"{item.Name}\"",
                    ex.Message);

            // Pre-load SVG assets for caption controls to eliminate lag on window resize
            _maximizeIconAsset =
                (IImage)SvgAssetValueConverter.Instance.Convert(
                    "avares://SysPilot/Assets/Icons/Window Controls Icons/Maximize.svg",
                    typeof(IImage),
                    null,
                    System.Globalization.CultureInfo.InvariantCulture)!;

            _restoreIconAsset =
                (IImage)SvgAssetValueConverter.Instance.Convert(
                    "avares://SysPilot/Assets/Icons/Window Controls Icons/Restore.svg",
                    typeof(IImage),
                    null,
                    System.Globalization.CultureInfo.InvariantCulture)!;

            PropertyChanged += (sender, e) =>
            {
                if (e.Property == WindowStateProperty)
                {
                    UpdateMaximizeIcon();
                }
            };

            Opened += async (_, _) =>
            {
                await InitializeCatalogAsync();
                _ = _updateService.CheckDownloadAndApplyAsync();
            };
        }

        #region Catalog

        private async Task InitializeCatalogAsync()
        {
            var remoteCatalog =
                await _catalogService.LoadCatalogAsync();

            if (remoteCatalog is null)
            {
                await ShowMessageAsync(
                    "Catalog Error",
                    "SysPilot could not load its application catalog.");

                return;
            }

            _categories =
                CatalogMapper.Map(remoteCatalog);

            if (NavList.Items.Count > 0 &&
                NavList.Items[0] is ListBoxItem firstItem)
            {
                NavList.SelectedItem = firstItem;
            }
        }

        #endregion

        #region Custom Title Bar & Caption Controls

        private void TitleBar_PointerPressed(
            object? sender,
            PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                if (e.ClickCount == 2)
                    ToggleMaximize();
                else
                    BeginMoveDrag(e);
            }
        }

        private void MinimizeButton_Click(
            object? sender,
            RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(
            object? sender,
            RoutedEventArgs e)
        {
            ToggleMaximize();
        }

        private void CloseButton_Click(
            object? sender,
            RoutedEventArgs e)
        {
            Close();
        }

        private void ToggleMaximize()
        {
            WindowState =
                WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;
        }

        private void UpdateMaximizeIcon()
        {
            MaximizeIconImage.Source =
                WindowState == WindowState.Maximized
                    ? _restoreIconAsset
                    : _maximizeIconAsset;
        }

        #endregion

        private void NavList_SelectionChanged(
            object? sender,
            SelectionChangedEventArgs e)
        {
            if (NavList.SelectedItem is ListBoxItem item &&
                item.Tag is string tag)
            {
                SettingsList.SelectedItem = null;
                ShowCategory(tag);
            }
        }

        private void SettingsList_SelectionChanged(
            object? sender,
            SelectionChangedEventArgs e)
        {
            if (SettingsList.SelectedItem is ListBoxItem)
            {
                NavList.SelectedItem = null;
                ShowSettings();
            }
        }

        private void ShowCategory(string tag)
        {
            var category =
                _categories.FirstOrDefault(c => c.Tag == tag);

            if (category is null)
                return;

            HeaderText.Text = category.Name;
            CardsControl.ItemsSource = category.Items;

            CategoryScrollViewer.IsVisible = true;
            SettingsPanel.IsVisible = false;
        }

        private void ShowSettings()
        {
            HeaderText.Text = "Settings";
            CardsControl.ItemsSource = null;

            CategoryScrollViewer.IsVisible = false;
            SettingsPanel.IsVisible = true;
        }

        private void OpenFolderButton_Click(
            object? sender,
            RoutedEventArgs e)
        {
            var localAppData =
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData);

            var sysPilotDir =
                Path.Combine(localAppData, "SysPilot");

            if (!Directory.Exists(sysPilotDir))
            {
                Directory.CreateDirectory(sysPilotDir);
            }

            try
            {
                Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = sysPilotDir,
                        UseShellExecute = true
                    });
            }
            catch (Exception ex)
            {
                _ = ShowMessageAsync(
                    "Error",
                    $"Could not open folder: {ex.Message}");
            }
        }

        private async void CheckForUpdatesButton_Click(
            object? sender,
            RoutedEventArgs e)
        {
            try
            {
                var updated = await _updateService.CheckDownloadAndApplyAsync();

                if (!updated)
                {
                    await ShowMessageAsync(
                        "No Updates",
                        "You're already running the latest version of SysPilot.");
                }
                // If updated == true, the app restarts itself — this code won't be reached.
            }
            catch (Exception ex)
            {
                await ShowMessageAsync(
                    "Update Check Failed",
                    $"Could not check for updates: {ex.Message}");
            }
        }

        private async void LaunchButton_Click(
            object? sender,
            RoutedEventArgs e)
        {
            if (sender is not Control control ||
                control.Tag is not AppItem item)
            {
                return;
            }

            var installDir =
                AppFileSystem.GetInstallDirectory(item);

            if (!string.IsNullOrWhiteSpace(item.ExecutablePath) &&
                !File.Exists(item.ExecutablePath))
            {
                item.ExecutablePath = string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(item.ExecutablePath))
            {
                var defaultDownloadPath =
                    AppFileSystem.GetDefaultInstallerPath(
                        installDir,
                        item);

                if (item.ExecutablePath.Equals(
                        defaultDownloadPath,
                        StringComparison.OrdinalIgnoreCase))
                {
                    var updatedPath =
                        AppFileSystem.FindExecutable(
                            installDir,
                            item);

                    if (!string.IsNullOrWhiteSpace(updatedPath) &&
                        updatedPath != item.ExecutablePath)
                    {
                        item.ExecutablePath = updatedPath;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(item.ExecutablePath))
            {
                if (string.IsNullOrWhiteSpace(item.DownloadUrl))
                {
                    await ShowMessageAsync(
                        $"No download URL set for \"{item.Name}\"",
                        "The application catalog does not contain a download URL for this item.");

                    return;
                }

                var resolvedPath =
                    await TryDownloadAsync(item);

                if (resolvedPath is null)
                    return;

                item.ExecutablePath = resolvedPath;
            }

            await _launchService.LaunchAsync(
                item,
                installDir);
        }

        private async Task<string?> TryDownloadAsync(
            AppItem item)
        {
            try
            {
                var result =
                    await _downloadService.EnsureDownloadedAsync(
                        item);

                if (result.WasAlreadyInProgress)
                    return null;

                if (result.NoExecutableWasFound)
                {
                    var installDir =
                        AppFileSystem.GetInstallDirectory(item);

                    await ShowMessageAsync(
                        $"Downloaded \"{item.Name}\"",
                        $"Saved to {installDir}, but no matching executable was found there.");

                    return null;
                }

                return result.ExecutablePath;
            }
            catch (DownloadFailedException ex)
            {
                if (!string.IsNullOrWhiteSpace(item.DownloadUrl))
                {
                    try
                    {
                        Process.Start(
                            new ProcessStartInfo
                            {
                                FileName = item.DownloadUrl,
                                UseShellExecute = true
                            });
                    }
                    catch
                    {
                        // Ignore failure to open the browser.
                    }
                }

                if (ex.IsBotProtection)
                {
                    await ShowMessageAsync(
                        $"Manual Action Required for \"{item.Name}\"",
                        $"This provider uses automated bot-detection that prevents direct downloading from the app.\n\n" +
                        $"The official download page has been opened in your browser. " +
                        $"Please save the file directly into:\n\n" +
                        $"{AppFileSystem.GetInstallDirectory(item)}");
                }
                else
                {
                    await ShowMessageAsync(
                        $"Manual Action Required for \"{item.Name}\"",
                        $"Error: {ex.Message}\n\n" +
                        $"The official download page has been opened in your default browser. " +
                        $"Please save the file into:\n\n" +
                        $"{AppFileSystem.GetInstallDirectory(item)}");
                }

                return null;
            }
        }

        private async Task ShowMessageAsync(
            string title,
            string message)
        {
            var dialog =
                new MessageDialog(title, message);

            await dialog.ShowDialog(this);
        }
    }
}