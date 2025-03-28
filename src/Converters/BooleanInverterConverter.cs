using System.Globalization;
using System.Windows.Data;

namespace TFLitePoseTrainer.Converters;

internal class BooleanInverterConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool booleanValue && !booleanValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool booleanValue && !booleanValue;
    }
}