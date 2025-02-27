using System;
using System.Linq;

namespace ERad5TestGUI.Converters
{
    public static class ByteExtension
    {
        public static byte[] GetBytes(short data, int byteOrder = 0)
        {
            return GetBytes(data, byteOrder == 1);
        }
        public static byte[] GetBytes(int data, int byteOrder = 0)
        {
            return GetBytes(data, byteOrder == 1);
        }
        public static byte[] GetBytes(short data, bool bigEndian = true)
        {
            if (!bigEndian)//因为计算机直接getbyte为小端，
            {
                return BitConverter.GetBytes(data);
            }
            else
            {
                var tmp = BitConverter.GetBytes(data);
                tmp.Reverse();
                return tmp;
            }
        }

        public static byte[] GetBytes(uint data, bool bigEndian = true)
        {
            if (!bigEndian)//大端
            {
                return BitConverter.GetBytes(data);
            }
            else
            {
                var tmp = BitConverter.GetBytes(data);
                tmp.Reverse();
                return tmp;
            }
        }
        public static byte[] GetBytes(int data, bool bigEndian = true)
        {
            if (!bigEndian)//大端
            {
                return BitConverter.GetBytes(data);
            }
            else
            {
                var tmp = BitConverter.GetBytes(data);
                tmp.Reverse();
                return tmp;
            }
        }

        public static short ToInt16(byte[] data, int startIndex, int byteorder = 0)
        {
            return ToInt16(data, startIndex, byteorder == 0);
        }

        public static short ToInt16(byte[] data, int startIndex, bool bigEndian = true)
        {
            if (bigEndian)//大端
            {
                return BitConverter.ToInt16(new byte[] { data[startIndex + 1], data[startIndex] }, 0);
            }
            else
            {
                return BitConverter.ToInt16(new byte[] { data[startIndex], data[startIndex + 1] }, 0);
            }
        }

        public static ushort ToUInt16(byte[] data, int startIndex, int byteorder = 0)
        {
            return ToUInt16(data, startIndex, byteorder == 0);
        }

        public static ushort ToUInt16(byte[] data, int startIndex, bool bigEndian = true)
        {
            if (bigEndian)//大端
            {
                return BitConverter.ToUInt16(new byte[] { data[startIndex + 1], data[startIndex] }, 0);
            }
            else
            {
                return BitConverter.ToUInt16(new byte[] { data[startIndex], data[startIndex + 1] }, 0);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="b"></param>
        /// <param name="indx"></param>
        /// <returns></returns>
        public static byte GetBit(this byte b, int indx)
        {
            return (byte)((b >> indx) & 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="b"></param>
        /// <param name="m">从右往左数,小的索引</param>
        /// <param name="n">从右往左数,大的索引</param>
        /// <returns></returns>
        public static byte GetBits(this byte b, int m, int n)
        {
            int mask = (1 << (n - m + 1)) - 1;
            byte result = (byte)((b >> m) & mask);
            return result;
        }

        public static bool TryConvertToIntHex(this string dataStr, out int data)
        {
            return int.TryParse(dataStr, System.Globalization.NumberStyles.HexNumber, null, out data);
        }
        public static bool TryConvertToUIntHex(this string dataStr, out uint data)
        {
            return uint.TryParse(dataStr, System.Globalization.NumberStyles.HexNumber, null, out data);
        }
        public static int ConvertToIntHex(this string dataStr)
        {
            return int.Parse(dataStr, System.Globalization.NumberStyles.HexNumber);
        }
        public static bool TryConvertToULongHex(this string dataStr, out ulong data)
        {
            return ulong.TryParse(dataStr, System.Globalization.NumberStyles.HexNumber, null, out data);
        }
        public static ulong ConvertToLongHex(this string dataStr)
        {
            return ulong.Parse(dataStr, System.Globalization.NumberStyles.HexNumber);
        }
        public static bool TryConvertToShortHex(this string dataStr, out short data)
        {
            return short.TryParse(dataStr, System.Globalization.NumberStyles.HexNumber, null, out data);
        }
        public static short ConvertToShortHex(this string dataStr)
        {
            return short.Parse(dataStr, System.Globalization.NumberStyles.HexNumber);
        }
        public static byte ConvertToByteHex(this string dataStr)
        {
            return byte.Parse(dataStr, System.Globalization.NumberStyles.HexNumber);
        }
        public static uint ConvertToUIntHex(this string dataStr)
        {
            return uint.Parse(dataStr, System.Globalization.NumberStyles.HexNumber);
        }


    

     

       
        
    }
    public interface IPipelineStep<INPUT, OUTPUT>
    {
        OUTPUT Process(INPUT input);
    }

    public class IntToStringStep : IPipelineStep<int, string>
    {
        public string Process(int input)
        {
            return input.ToString();
        }
    }
    public static class PipelineStepExtensions
    {
        public static OUTPUT Convert<INPUT, OUTPUT>(this INPUT input, IPipelineStep<INPUT, OUTPUT> step)
        {
            return step.Process(input);
        }

    }

    public class DoubleToIntSetp : IPipelineStep<double, int>
    {
        public int Process(double input)
        {
            return Convert.ToInt32(input);
        }
    }
    /// <summary>
    /// uint32 byte 反转
    /// </summary>
    public class UInt32Reverse : IPipelineStep<uint, uint>
    {
        public uint Process(uint input)
        {
            byte[] bytes = BitConverter.GetBytes(input);
            Array.Reverse(bytes); // 反转字节数组中的字节顺序
            uint convertedValue = BitConverter.ToUInt32(bytes, 0); // 将字节数组转换为整型数据
                                                                   //uint output = ((input & 0xff) << 24) | ((input & 0xff00) << 8) | ((input & 0xff0000) >> 8) | ((input & 0xff000000) >> 24);
            return convertedValue;
        }
    }

    public class UInt3216Reverse : IPipelineStep<uint, uint>
    {
        public uint Process(uint input)
        {
            byte[] bytes = BitConverter.GetBytes(input);
            Array.Reverse(bytes); // 反转字节数组中的字节顺序
            uint convertedValue = BitConverter.ToUInt32(bytes, 0); // 将字节数组转换为整型数据
                                                                   //00 08 a0 00->a0 00 00 08
                                                                   //0x080000a0 
                                                                   //把 23和01 调换
            byte temp;
            byte temp1;
            temp = bytes[0];
            temp1 = bytes[1];
            bytes[0] = bytes[2];
            bytes[1] = bytes[3];
            bytes[2] = temp;
            bytes[3] = temp1;
            return BitConverter.ToUInt32(bytes, 0);
        }
    }
}
