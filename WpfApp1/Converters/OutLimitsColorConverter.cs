using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ERad5TestGUI.Converters
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

    public class ClearCommandParamtersConvert : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.Clone();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
