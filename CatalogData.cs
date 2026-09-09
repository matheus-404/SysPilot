using System.Collections.Generic;

namespace SysPilot.Models
{
    public class AppCategory
    {
        public string Name { get; set; } = string.Empty;
        public string Tag { get; set; } = string.Empty;
        public List<AppItem> Items { get; set; } = new();
    }

    public static class CatalogData
    {
        public static List<AppCategory> GetCategories() => new()
        {
            new AppCategory
            {
                Name = "System Tweaks",
                Tag = "SystemTweaks",
                Items = new List<AppItem>
                {
                    new()
                    {
                        Name = "Process Explorer",
                        IconPath = "avares://SysPilot/Assets/Icons/System Tweaks Icons/ProcessExplorer_Icon.svg",
                        DownloadUrl = "https://download.sysinternals.com/files/ProcessExplorer.zip"
                    },
                    new()
                    {
                        Name = "Serviwin",
                        IconPath = "avares://SysPilot/Assets/Icons/System Tweaks Icons/Serviwin_Icon.svg",
                        DownloadUrl = "https://www.nirsoft.net/utils/serviwin.zip"
                    },
                    new()
                    {
                        Name = "Autoruns",
                        IconPath = "avares://SysPilot/Assets/Icons/System Tweaks Icons/Autoruns_Icon.svg",
                        DownloadUrl = "https://download.sysinternals.com/files/Autoruns.zip"
                    },
                    new()
                    {
                        Name = "BCUninstaller",
                        IconPath = "avares://SysPilot/Assets/Icons/System Tweaks Icons/BCUninstaller_Icon.svg",
                        DownloadUrl = "https://github.com/BCUninstaller/Bulk-Crap-Uninstaller/releases/download/v6.2/BCUninstaller_6.2.0_portable.zip"
                    },
                    new()
                    {
                        Name = "Defender Remover",
                        IconPath = "avares://SysPilot/Assets/Icons/System Tweaks Icons/DefenderRemover_Icon.svg",
                        DownloadUrl = "https://github.com/ionuttbara/windows-defender-remover/archive/refs/tags/release13-rev1.zip",
                        DownloadExtension = ".zip"
                    }
                }
            },
            new AppCategory
            {
                Name = "GPU & Display",
                Tag = "GpuDisplay",
                Items = new List<AppItem>
                {
                    new()
                    {
                        Name = "DDU",
                        IconPath = "avares://SysPilot/Assets/Icons/GPU & Display Icons/DDU_Icon.svg",
                        DownloadUrl = "https://download.wagnardsoft.com/DDU/DDU%20v18.1.5.7.exe",
                        DownloadExtension = ".exe"
                    },
                    new()
                    {
                        Name = "NVCleanstall",
                        IconPath = "avares://SysPilot/Assets/Icons/GPU & Display Icons/NVCleanstall_Icon.svg",
                        DownloadUrl = "https://www.techpowerup.com/download/techpowerup-nvcleanstall/",
                        DownloadExtension = ".exe"
                    },
                    new()
                    {
                        Name = "Radeon Software Slimmer",
                        IconPath = "avares://SysPilot/Assets/Icons/GPU & Display Icons/Radeon_Slimmer_Icon.svg",
                        DownloadUrl = "https://github.com/GSDragoon/RadeonSoftwareSlimmer/releases/download/1.12.0/RadeonSoftwareSlimmer_1.12.0_net90.zip"
                    },
                    new()
                    {
                        Name = "MSI Afterburner",
                        IconPath = "avares://SysPilot/Assets/Icons/GPU & Display Icons/MSI_Afterburner_Icon.svg",
                        DownloadUrl = "https://download.msi.com/uti_exe/vga/MSIAfterburnerSetup.zip",
                        DownloadExtension = ".zip"
                    },
                    new()
                    {
                        Name = "CRU",
                        IconPath = "avares://SysPilot/Assets/Icons/GPU & Display Icons/CRU_Icon.svg",
                        DownloadUrl = "https://www.monitortests.com/download/cru/cru-1.5.3.zip"
                    }
                }
            },
            new AppCategory
            {
                Name = "Core Utilities",
                Tag = "CoreUtilities",
                Items = new List<AppItem>
                {
                    new()
                    {
                        Name = "7-Zip",
                        IconPath = "avares://SysPilot/Assets/Icons/Core Utilities Icons/7-Zip_Icon.svg",
                        DownloadUrl = "https://github.com/ip7z/7zip/releases/download/26.02/7z2602-x64.exe",
                        DownloadExtension = ".exe"
                    },
                    new()
                    {
                        Name = "VC++ AiO Redist.",
                        IconPath = "avares://SysPilot/Assets/Icons/Core Utilities Icons/Visual_Studio_Icon_2026.svg",
                        DownloadUrl = "https://github.com/abbodi1406/vcredist/releases/download/v0.105.0/VisualCppRedist_AIO_x86_x64.exe",
                        DownloadExtension = ".exe"
                    },
                    new()
                    {
                        Name = ".NET",
                        IconPath = "avares://SysPilot/Assets/Icons/Core Utilities Icons/NET_Icon.svg",
                        DownloadUrl = "https://builds.dotnet.microsoft.com/dotnet/Sdk/10.0.400/dotnet-sdk-10.0.400-win-x64.exe",
                        DownloadExtension = ".exe"
                    },
                    new()
                    {
                        Name = "DirectX End-User Runtime Web Installer",
                        IconPath = "avares://SysPilot/Assets/Icons/Core Utilities Icons/DirectX_Icon.svg",
                        DownloadUrl = "https://download.microsoft.com/download/1/7/1/1718ccc4-6315-4d8e-9543-8e28a4e18c4c/dxwebsetup.exe",
                        DownloadExtension = ".exe"
                    }
                }
            },
            new AppCategory
            {
                Name = "Apps",
                Tag = "Apps",
                Items = new List<AppItem>
                {
                    new()
                    {
                        Name = "Firefox",
                        IconPath = "avares://SysPilot/Assets/Icons/Apps Icons/Firefox_Icon.svg",
                        DownloadUrl = "https://download.mozilla.org/?product=firefox-stub&os=win&lang=en-US",
                        DownloadExtension = ".exe"
                    },
                    new()
                    {
                        Name = "LibreWolf",
                        IconPath = "avares://SysPilot/Assets/Icons/Apps Icons/LibreWolf_Icon.svg",
                        DownloadUrl = "https://dl.librewolf.net/librewolf/154.0.1-3/librewolf-154.0.1-3-windows-x86_64-setup.exe",
                        DownloadExtension = ".exe"
                    },
                    new()
                    {
                        Name = "VLC",
                        IconPath = "avares://SysPilot/Assets/Icons/Apps Icons/VLC_Icon.svg",
                        DownloadUrl = "https://get.videolan.org/vlc/3.0.23/win32/vlc-3.0.23-win32.exe",
                        DownloadExtension = ".exe"
                    },
                    new()
                    {
                        Name = "qView",
                        IconPath = "avares://SysPilot/Assets/Icons/Apps Icons/qView_Icon.svg",
                        DownloadUrl = "https://github.com/jurplel/qView/releases/download/7.1/qView-7.1-win64.exe",
                        DownloadExtension = ".exe"
                    },
                    new()
                    {
                        Name = "Spotify",
                        IconPath = "avares://SysPilot/Assets/Icons/Apps Icons/Spotify_Icon.svg",
                        DownloadUrl = "https://download.scdn.co/SpotifySetup.exe",
                        DownloadExtension = ".exe"
                    },
                    new()
                    {
                        Name = "Steam",
                        IconPath = "avares://SysPilot/Assets/Icons/Apps Icons/Steam_Icon.svg",
                        DownloadUrl = "https://cdn.akamai.steamstatic.com/client/installer/SteamSetup.exe",
                        DownloadExtension = ".exe"
                    },
                    new()
                    {
                        Name = "Epic Games",
                        IconPath = "avares://SysPilot/Assets/Icons/Apps Icons/Epic_Games_Icon.svg",
                        DownloadUrl = "https://launcher-public-service-prod06.ol.epicgames.com/launcher/api/installer/download/EpicGamesLauncherInstaller.exe",
                        DownloadExtension = ".exe"
                    },
                    new()
                    {
                        Name = "Discord",
                        IconPath = "avares://SysPilot/Assets/Icons/Apps Icons/Discord_Icon.svg",
                        DownloadUrl = "https://discord.com/api/downloads/distributions/app/installers/latest?channel=stable&platform=win&arch=x64",
                        DownloadExtension = ".exe"
                    },
                    new()
                    {
                        Name = "LibreOffice",
                        IconPath = "avares://SysPilot/Assets/Icons/Apps Icons/LibreOffice_Icon.svg",
                        DownloadUrl = "https://download.documentfoundation.org/libreoffice/stable/26.8.0/win/x86_64/LibreOffice_26.8.0_Win_x86-64.msi",
                        DownloadExtension = ".msi"
                    }
                }
            }
        };
    }
}