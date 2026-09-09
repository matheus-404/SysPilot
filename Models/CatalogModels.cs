using System.Collections.Generic;

namespace SysPilot.Models;

public sealed class RemoteCatalog
{
    public int CatalogVersion { get; set; }

    public List<RemoteCategory> Categories { get; set; } = new();
}

public sealed class RemoteCategory
{
    public string Name { get; set; } = string.Empty;

    public string Tag { get; set; } = string.Empty;

    public List<RemoteItem> Items { get; set; } = new();
}

public sealed class RemoteItem
{
    public string Name { get; set; } = string.Empty;

    public string? IconPath { get; set; }

    public string? DownloadUrl { get; set; }

    public string DownloadExtension { get; set; } = ".zip";

    public string Arguments { get; set; } = string.Empty;
}