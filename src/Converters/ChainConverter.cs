using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;

namespace TFLitePoseTrainer.Converters;

internal class ChainConverter : IValueConverter
{
    IValueConverter[] _converters = [];
    internal ObservableCollection<IValueConverter> Converters { get; } = [];

    internal ChainConverter()
    {
        Converters.CollectionChanged += (sender, e) => _converters = [.. Converters];
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var convertedValue = value;

        for (int i = 0; i < _converters.Length; i++)
        {
            convertedValue = _converters[i].Convert(convertedValue, targetType, parameter, culture);
        }

        return convertedValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var convertedValue = value;

        for (int i = _converters.Length - 1; i >= 0; i--)
        {
            convertedValue = _converters[i].ConvertBack(convertedValue, targetType, parameter, culture);
        }

        return convertedValue;
    }
}
