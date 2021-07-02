using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CurrentMonitor.WPF.Converters
{
    public class WidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(targetType != typeof(GridLength) | targetType != typeof(double)))
            {
                throw new InvalidOperationException("Value must be Double");
            }

            if (parameter == null)
            {
                throw new InvalidOperationException("Converter parameter must not be null");
            }

            if (!double.TryParse(parameter.ToString(), out double parameterValue)) throw new InvalidOperationException("ConverterParameter must be Double");

            return (double)value - parameterValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
