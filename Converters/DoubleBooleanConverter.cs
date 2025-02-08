using System;
using System.Globalization;
using System.Windows.Data;

namespace WpfApp1.Converters
{
    public class DoubleBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double valD && valD == 1)
                return true;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool valB && valB)
                return 1d;
            return 0d;
        }
    }

    public class InvertBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool valb)
                return !valb;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool valB)
                return !valB;
            return false;
        }
    }
}
