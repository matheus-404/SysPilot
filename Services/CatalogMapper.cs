using System;
using System.Collections.Generic;
using SysPilot.Models;

namespace SysPilot.Services;

public static class CatalogMapper
{
    public static List<AppCategory> Map(RemoteCatalog catalog)
    {
        var categories = new List<AppCategory>();

        foreach (var remoteCategory in catalog.Categories)
        {
            var category = new AppCategory
            {
                Name = remoteCategory.Name,
                Tag = remoteCategory.Tag
            };

            foreach (var remoteItem in remoteCategory.Items)
            {
                if (!Uri.TryCreate(
                        remoteItem.DownloadUrl,
                        UriKind.Absolute,
                        out var downloadUri)
                    && !string.IsNullOrWhiteSpace(remoteItem.DownloadUrl))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(remoteItem.DownloadUrl)
                    && downloadUri!.Scheme != Uri.UriSchemeHttps)
                {
                    continue;
                }

                category.Items.Add(new AppItem
                {
                    Name = remoteItem.Name,
                    IconPath = remoteItem.IconPath,
                    DownloadUrl = remoteItem.DownloadUrl,
                    DownloadExtension = remoteItem.DownloadExtension,
                    Arguments = remoteItem.Arguments
                });
            }

            categories.Add(category);
        }

        return categories;
    }
}