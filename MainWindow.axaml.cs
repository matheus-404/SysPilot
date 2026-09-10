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

        private readonly AppDownloadService _downloadService = new();
        private readonly AppLaunchService _launchService;
        private readonly CatalogService _catalogService = new();
        private readonly AppUpdateService _updateService = new();

        private readonly IImage _maximizeIconAsset;
        private readonly IImage _restoreIconAsset;

        public MainWindow()
        {
            InitializeComponent();

            if (!Design.IsDesignMode)
            {
                _updateService = new AppUpdateService();
            }

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
                    UpdateResizeGripsEnabled();
                }
            };

            AttachResizeGrips();

            Opened += async (_, _) =>
            {
                await InitializeCatalogAsync();

                try
                {
                    _ = await _updateService.CheckDownloadAndApplyAsync();
                    // If an update was found, the app restarts itself and this line never runs.
                    // If no update, execution just continues normally — no dialog shown.
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Startup update check failed: {ex}");
                }
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

            // Select the first category after the catalog has loaded.
            if (NavList.Items.Count > 0 &&
                NavList.Items[0] is ListBoxItem firstItem)
            {
                NavList.SelectedItem = firstItem;
            }
        }

        #endregion

        #region Custom Title Bar & Caption Controls

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

        #region Manual Resize Grips

        private readonly List<Border> _resizeGrips = new();

        private void AttachResizeGrips()
        {
            AttachResizeGrip("ResizeTop", WindowEdge.North);
            AttachResizeGrip("ResizeBottom", WindowEdge.South);
            AttachResizeGrip("ResizeLeft", WindowEdge.West);
            AttachResizeGrip("ResizeRight", WindowEdge.East);
            AttachResizeGrip("ResizeTopLeft", WindowEdge.NorthWest);
            AttachResizeGrip("ResizeTopRight", WindowEdge.NorthEast);
            AttachResizeGrip("ResizeBottomLeft", WindowEdge.SouthWest);
            AttachResizeGrip("ResizeBottomRight", WindowEdge.SouthEast);

            UpdateResizeGripsEnabled();
        }

        private void AttachResizeGrip(string name, WindowEdge edge)
        {
            var grip = this.FindControl<Border>(name);

            if (grip is null)
                return;

            _resizeGrips.Add(grip);

            grip.PointerPressed += (_, e) =>
            {
                if (WindowState == WindowState.Maximized)
                    return;

                if (e.GetCurrentPoint(grip).Properties.IsLeftButtonPressed)
                {
                    BeginResizeDrag(edge, e);
                }
            };
        }

        private void UpdateResizeGripsEnabled()
        {
            var enabled = WindowState != WindowState.Maximized;

            foreach (var grip in _resizeGrips)
            {
                grip.IsHitTestVisible = enabled;
            }
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