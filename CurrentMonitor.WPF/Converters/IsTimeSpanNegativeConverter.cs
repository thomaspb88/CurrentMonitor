using System;
using System.Globalization;
using System.Windows.Data;

namespace CurrentMonitor.WPF.Converters
{
    public class IsTimeSpanNegativeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (TimeSpan.TryParse((string)value, out _))
            {
                return TimeSpan.Parse((string)value).TotalSeconds < 0;
            }
            return false;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
