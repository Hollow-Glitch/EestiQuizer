using System.Globalization;
using System.Windows.Data;


namespace EestiQuizer.Common;


public class EnumerableToStringValueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is IEnumerable<string> collection) {
            return collection.StringJoin(" ");
        } else {
            return string.Empty;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("One-way conversion only.");
    }
}

