using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace ERad5TestGUI.Converters
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

    public class DeviceStartConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //Started
            if (value is bool valB)
            {
                return valB ? "Stop" : "Start";
            }
            return "Start";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
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

    public class DoubleHexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (double.TryParse(value.ToString(), out double dVal))
            {
                int iVal = (int)dVal;
                return iVal.ToString("X");
            }
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (int.TryParse(value.ToString(), NumberStyles.HexNumber, null, out int iVal))
            {
                return (double)iVal;
            }
            return 0d;
        }
    }

    public class DoubleToByteHexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (double.TryParse(value.ToString(), out double dVal))
            {
                int iVal = (int)dVal;
                return iVal.ToString("X");
            }
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (int.TryParse(value.ToString(), NumberStyles.HexNumber, null, out int iVal))
            {
                if (iVal > byte.MaxValue)
                    return 0xFF;
                return (double)iVal;
            }
            return 0d;
        }
    }

    public class BooleanInvertVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool valB)
            {
                return valB ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EnumerableNullReplaceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var collection = (IEnumerable)value;
            var list = collection
               .Cast<object>()
               .ToList();
            list.Insert(0, null);
            return
               list.Cast<object>()
                         .Select(x => x ?? parameter)
                         .ToArray();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
