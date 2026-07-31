using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Agentic.Desktop.Converters;

/// <summary>
/// Converts a bool to <see cref="Visibility"/>.
/// Use ConverterParameter="Invert" to invert the logic.
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool b)
        {
            if (parameter is string s && s.Equals("Invert", StringComparison.OrdinalIgnoreCase))
                b = !b;
            return b ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
