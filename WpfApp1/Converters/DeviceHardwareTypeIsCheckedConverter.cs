using System;
using System.Globalization;
using System.Windows.Data;

namespace WpfApp1.Converters
{
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
}
