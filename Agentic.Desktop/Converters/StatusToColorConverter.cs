using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace Agentic.Desktop.Converters;

/// <summary>
/// Converts ConnectionState (int: 0=Disconnected, 1=Connecting, 2=Connected) to a color.
/// </summary>
public class StatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int state)
        {
            return state switch
            {
                2 => new SolidColorBrush(Colors.Green),    // Connected
                1 => new SolidColorBrush(Colors.Orange),   // Connecting
                _ => new SolidColorBrush(Colors.Gray),     // Disconnected
            };
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
