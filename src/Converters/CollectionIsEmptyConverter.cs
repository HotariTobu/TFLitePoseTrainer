using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace TFLitePoseTrainer.Converters;

internal class CollectionIsEmptyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is IList collection && collection.Count == 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("Cannot convert back in CollectionIsEmptyConverter.");
    }
}
