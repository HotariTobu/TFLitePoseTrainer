using System.Globalization;
using System.Windows.Data;

namespace TFLitePoseTrainer.Converters;

internal class IsZeroConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        switch (value)
        {
            case sbyte sb:
                return sb == 0;
            case byte b:
                return b == 0;
            case short s:
                return s == 0;
            case ushort us:
                return us == 0;
            case int i:
                return i == 0;
            case uint ui:
                return ui == 0;
            case long l:
                return l == 0;
            case ulong ul:
                return ul == 0;
            case float f:
                return f == 0;
            case double d:
                return d == 0;
            case decimal de:
                return de == 0;
        }

        throw new ArgumentException("Value is not a number.");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("Cannot convert back in IsZeroConverter.");
    }
}
