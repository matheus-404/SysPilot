using System;
using System.Collections.Concurrent;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Platform;
using Avalonia.Svg.Skia;

namespace SysPilot.Converters
{
    public class SvgAssetValueConverter : IValueConverter
    {
        public static readonly SvgAssetValueConverter Instance = new();

        // Cache parsed SVGs to eliminate redundant stream loading and Skia parsing
        private static readonly ConcurrentDictionary<string, SvgImage> s_svgCache = new(StringComparer.OrdinalIgnoreCase);

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string rawUri && !string.IsNullOrWhiteSpace(rawUri))
            {
                if (s_svgCache.TryGetValue(rawUri, out var cachedImage))
                {
                    return cachedImage;
                }

                try
                {
                    var uri = new Uri(rawUri);
                    using var stream = AssetLoader.Open(uri);
                    var source = SvgSource.LoadFromStream(stream);

                    if (source != null)
                    {
                        var image = new SvgImage { Source = source };
                        s_svgCache.TryAdd(rawUri, image);
                        return image;
                    }
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}