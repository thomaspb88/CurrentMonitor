using System;
using System.Windows;
using System.Windows.Data;

namespace CurrentMonitor.WPF.Converters
{
    public class MarginConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(Double))
            {
                throw new InvalidOperationException("Value must be Double");
            }

            return new Thickness(System.Convert.ToDouble(value), 0, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
