using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SysPilot.Models;

namespace SysPilot.Services;

public sealed class CatalogService
{
    private const string RemoteCatalogUrl =
        "https://raw.githubusercontent.com/matheus-404/SysPilot/master/Catalog/catalog.json";

    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CatalogService()
    {
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            "SysPilot");
    }

    public async Task<RemoteCatalog?> LoadCatalogAsync(
        CancellationToken cancellationToken = default)
    {
        var remoteCatalog = await TryLoadRemoteCatalogAsync(
            cancellationToken);

        if (remoteCatalog is not null)
            return remoteCatalog;

        return await TryLoadLocalCatalogAsync(
            cancellationToken);
    }

    private async Task<RemoteCatalog?> TryLoadRemoteCatalogAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            var cacheBustedUrl =
                $"{RemoteCatalogUrl}?t={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

            using var response = await _httpClient.GetAsync(
                cacheBustedUrl,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            await using var stream =
                await response.Content.ReadAsStreamAsync(
                    cancellationToken);

            var catalog =
                await JsonSerializer.DeserializeAsync<RemoteCatalog>(
                    stream,
                    JsonOptions,
                    cancellationToken);

            return ValidateCatalog(catalog);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Remote catalog failed: {ex}");

            return null;
        }
    }

    private static async Task<RemoteCatalog?> TryLoadLocalCatalogAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            var catalogPath = Path.Combine(
                AppContext.BaseDirectory,
                "Catalog",
                "catalog.json");

            if (!File.Exists(catalogPath))
                return null;

            await using var stream =
                File.OpenRead(catalogPath);

            var catalog =
                await JsonSerializer.DeserializeAsync<RemoteCatalog>(
                    stream,
                    JsonOptions,
                    cancellationToken);

            return ValidateCatalog(catalog);
        }
        catch
        {
            return null;
        }
    }

    private static RemoteCatalog? ValidateCatalog(
        RemoteCatalog? catalog)
    {
        if (catalog is null)
            return null;

        if (catalog.Categories.Count == 0)
            return null;

        foreach (var category in catalog.Categories)
        {
            if (string.IsNullOrWhiteSpace(category.Name))
                return null;

            if (string.IsNullOrWhiteSpace(category.Tag))
                return null;

            foreach (var item in category.Items)
            {
                if (string.IsNullOrWhiteSpace(item.Name))
                    return null;

                if (!string.IsNullOrWhiteSpace(item.DownloadUrl))
                {
                    if (!Uri.TryCreate(
                            item.DownloadUrl,
                            UriKind.Absolute,
                            out var uri))
                    {
                        return null;
                    }

                    if (uri.Scheme != Uri.UriSchemeHttps)
                        return null;
                }
            }
        }

        return catalog;
    }
}