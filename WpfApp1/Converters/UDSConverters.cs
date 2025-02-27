using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using ERad5TestGUI.UDS;

namespace ERad5TestGUI.Converters
{
    public class ColorMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length == 0)
            {
                return DependencyProperty.UnsetValue;
            }
            UDSResponse response = (UDSResponse)values[0];
            ServerStatus status = (ServerStatus)values[1];
            SolidColorBrush solidBrush = new SolidColorBrush();
            if (status == ServerStatus.Done)
            {
                if (response == UDSResponse.Negative)
                {
                    solidBrush.Color = Colors.Red;
                }
                else//positive
                {
                    solidBrush.Color = Colors.Green;
                }
            }
            else
            {
                solidBrush.Color = Colors.Gray;
            }

            return solidBrush;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return DependencyProperty.UnsetValue;
            }
            UDSResponse response = (UDSResponse)value;
            //ServerStatus status = (ServerStatus)value;
            SolidColorBrush solidBrush = new SolidColorBrush();

            switch (response)
            {
                case UDSResponse.Init:
                    solidBrush.Color = Colors.LightGray;
                    break;
                case UDSResponse.Pass:
                case UDSResponse.Positive:
                    solidBrush.Color = Colors.Green;
                    break;
                case UDSResponse.Negative:
                case UDSResponse.Timeout:
                case UDSResponse.ParseError:
                case UDSResponse.Unknow:
                    solidBrush.Color = Colors.OrangeRed;
                    break;
                case UDSResponse.FlowControl:
                    solidBrush.Color = Colors.LightGreen;
                    break;
                default:
                    return DependencyProperty.UnsetValue;
            }

            return solidBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class Int2StringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int.TryParse(value.ToString(), out int x);
            return x.ToString("X");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!uint.TryParse(value.ToString(), System.Globalization.NumberStyles.HexNumber, null, out uint startID))
            {
                startID = 0;
            }
            return startID;
        }
    }

    public class Enable2ReadOnlyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool.TryParse(value.ToString(), out bool x);
            return !x;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool.TryParse(value.ToString(), out bool x);
            return !x;
        }
    }

    public class ProgressStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int.TryParse(value.ToString(), out int progress);
            return progress < 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
