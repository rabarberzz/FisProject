using System.Globalization;

namespace ControllerApp.Converters
{
    public class MetersToKilometersConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double meters)
            {
                return meters / 1000;
            }
            return 0;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double kilometers)
            {
                return kilometers * 1000;
            }
            return 0;
        }
    }
}
