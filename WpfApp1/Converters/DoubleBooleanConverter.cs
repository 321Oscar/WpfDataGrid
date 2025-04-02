using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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

    /// <summary>
    /// Converts numeric and byte array instances to <see cref="string" /> hex instances.
    /// </summary>
    [ValueConversion(typeof(ulong), typeof(string))]
    [ValueConversion(typeof(long), typeof(string))]
    [ValueConversion(typeof(uint), typeof(string))]
    [ValueConversion(typeof(int), typeof(string))]
    [ValueConversion(typeof(ushort), typeof(string))]
    [ValueConversion(typeof(short), typeof(string))]
    [ValueConversion(typeof(byte), typeof(string))]
    [ValueConversion(typeof(byte[]), typeof(string))]
    public class HexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ulong u64) return $"0x{u64:X16}";
            if (value is long i64) return $"0x{i64:X16}";
            if (value is uint u32) return $"0x{u32:X8}";
            if (value is int i32) return $"0x{i32:X8}";
            if (value is ushort u16) return $"0x{u16:X4}";
            if (value is short i16) return $"0x{i16:X4}";
            if (value is byte b) return $"0x{b:X2}";
            if (value is byte[] bArray)
            {
                return BitConverter.ToString(bArray).Replace("-", " ");
            }
            throw new InvalidDataException("Value cannot be converted to hex");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s)
            {
                if (targetType == typeof(ulong))
                    return ulong.Parse(s.Replace("0x", ""), NumberStyles.HexNumber);
                if (targetType == typeof(long))
                    return long.Parse(s.Replace("0x", ""), NumberStyles.HexNumber);
                if (targetType == typeof(uint))
                    return uint.Parse(s.Replace("0x", ""), NumberStyles.HexNumber);
                if (targetType == typeof(int))
                    return int.Parse(s.Replace("0x", ""), NumberStyles.HexNumber);
                if (targetType == typeof(ushort))
                    return ushort.Parse(s.Replace("0x", ""), NumberStyles.HexNumber);
                if (targetType == typeof(short))
                    return short.Parse(s.Replace("0x", ""), NumberStyles.HexNumber);
                if (targetType == typeof(byte))
                    return byte.Parse(s.Replace("0x", ""), NumberStyles.HexNumber);
                if (targetType == typeof(byte[]))
                {
                    string[] hexValues = Regex.Split(s.Replace("0x", ""), "[^0-9A-Fa-f]+");
                    List<byte> byteList = new List<byte>();
                    foreach (string hexValue in hexValues)
                    {
                        if (byte.TryParse(hexValue, NumberStyles.AllowHexSpecifier, null, out byte b))
                        {
                            byteList.Add(b);
                        }
                    }

                    return byteList.ToArray();
                }
            }

            throw new InvalidDataException("Value cannot be converted from hex");
        }
    }
    /// <summary>
    /// Current Pregress and Max
    /// </summary>
    public class ProgressToPercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int current && parameter is double max && max != 0)
            {
                if (current > max)
                    return "100%";

                double percentage = (double)current / max * 100;
                return $"{percentage:F2}%"; // 格式化为两位小数
            }
            return "0%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
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
