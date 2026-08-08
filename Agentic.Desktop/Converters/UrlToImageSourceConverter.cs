using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Agentic.Desktop.Converters;

/// <summary>
/// Converts a remote icon URL (string) to an <see cref="ImageSource"/>.
/// SVG URLs use <see cref="SvgImageSource"/> with explicit rasterization size;
/// anything else falls back to <see cref="BitmapImage"/>.
/// Returns <c>null</c> for invalid or empty URLs so the caller can show a fallback.
/// </summary>
public class UrlToImageSourceConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not string url || string.IsNullOrWhiteSpace(url))
            return null!;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return null!;

        try
        {
            if (url.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
            {
                return new SvgImageSource(uri);
            }

            return new BitmapImage(uri)
            {
                DecodePixelWidth = 64,
                DecodePixelType = DecodePixelType.Logical
            };
        }
        catch
        {
            return null!;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
