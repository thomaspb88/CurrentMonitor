using System;
using System.Globalization;
using System.Windows.Data;

namespace CurrentMonitor.WPF.Converters
{
    public class DoubleMultiplierConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result1 = (double)value;
            if (double.TryParse((string)parameter, out double result2))
            {

            }

            return result1 * result2;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
