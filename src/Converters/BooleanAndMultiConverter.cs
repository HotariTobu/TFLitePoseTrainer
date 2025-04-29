using System.Globalization;
using System.Windows.Data;

namespace TFLitePoseTrainer.Converters;

internal class BooleanAndMultiConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        return values.All(value => value is bool boolean && boolean);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("Cannot convert back in BooleanAndMultiConverter.");
    }
}
