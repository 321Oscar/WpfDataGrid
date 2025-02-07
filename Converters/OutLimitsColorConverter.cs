using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace WpfApp1.Converters
{
    public class OutLimitsColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var brush = new SolidColorBrush();

            if (value is bool boolValue && boolValue)
            {
                brush.Color = Color.FromRgb(254, 148, 130);
            }
            else
            {
                brush.Color = Color.FromRgb(166, 217, 34);
            }

            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class UpdateSourceTriggerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue)
            {
                return UpdateSourceTrigger.Explicit;//
            }
            else
            {
                return UpdateSourceTrigger.PropertyChanged;//brush.Color = Color.FromRgb(166, 217, 34);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DeviceHardwareTypeIsCheckedConverter : IValueConverter
    {
        //public static SexToIsCheckedCvt Cvt = new SexToIsCheckedCvt();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() == parameter?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (bool.TryParse(value?.ToString(), out var isChecked) &&
                Enum.TryParse<Devices.DeviceHardWareType>(parameter?.ToString(), out var param))
            {
                if (isChecked)
                {
                    return param;
                }
            }
            return Devices.DeviceHardWareType.None;
        }
    }

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
}
