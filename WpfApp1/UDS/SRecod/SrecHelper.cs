using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ERad5TestGUI.UDS.SRecord
{
    public class SrecHelper
    {
        /// <summary>
        /// 数据长度
        /// </summary>
        public static byte Linelen = 0x10;
        /// <summary>
        /// 地址长度
        /// </summary>
        public static byte Addrlen = 0x4;
        /// <summary>
        /// CRC长度
        /// </summary>
        public static byte Crcrlen = 0x1;
        /// <summary>
        /// 文件类型
        /// </summary>
        public static string RecordType = "S3";
        /// <summary>
        /// 文件结束标记
        /// </summary>
        public static List<string> cmdEndStrs = new List<string>() { "S5","S7", "S8", "S9" };

        /// <summary>
        /// 0:不做大小端转换；1：转换
        /// </summary>
        public static string ByteOrder = "0";
        public static byte AddCrcrlen => (byte)(Addrlen + Crcrlen);

        public static string TransForm(byte[] data, UInt32 startPosition,string recordType)
        {
            StringBuilder result = new StringBuilder();
            int RdPtr = 0;
            byte crc = 0;
            string writeline = string.Empty;
            //UInt32 HwVerCfgrposition = 0xFC0400;
            writeline = GetLineHeaderByRecordType(data.Length, RdPtr, Linelen, AddCrcrlen, Addrlen, startPosition, recordType);

            while (RdPtr < data.Length)
            {
                byte cpldbindata = data[RdPtr];
                RdPtr++;
                writeline = writeline + Convert.ToString(cpldbindata, 16).PadLeft(2, '0').ToUpper();
                crc = (byte)(crc + cpldbindata);
                if ((RdPtr % Linelen) == 0)
                {
                    crc = (byte)(0xff - (byte)(crc + GetByteCRCByRecordType(startPosition, Linelen, AddCrcrlen, recordType)));
                    sbyte crctemp = (sbyte)crc;
                    if (crctemp < 0)
                    {
                        crc = (byte)(sbyte.MaxValue - sbyte.MinValue + crctemp + 1);
                    }
                    writeline = writeline + Convert.ToString(crc, 16).PadLeft(2, '0').ToUpper();
                    result.AppendLine(writeline);
                    //Mergesr.WriteLine(writeline);
                    startPosition = startPosition + Linelen;
                    writeline = GetLineHeaderByRecordType(data.Length, RdPtr, Linelen, AddCrcrlen, Addrlen, startPosition, recordType);

                    crc = 0;
                }
            }

            if ((data.Length % Linelen) > 0)
            {
                crc = (byte)(0xff - (byte)(crc + GetByteCRCByRecordType(data.Length, startPosition, Linelen, AddCrcrlen, recordType)));
                sbyte crctemp = (sbyte)crc;
                if (crctemp < 0)
                {
                    crc = (byte)(sbyte.MaxValue - sbyte.MinValue + crctemp + 1);
                }
                writeline = writeline + Convert.ToString(crc, 16).PadLeft(2, '0').PadLeft(2, '0').ToUpper();
                result.AppendLine(writeline);
                // Mergesr.WriteLine(writeline);
                startPosition = startPosition + Linelen;
                //writeline = "S3" + Convert.ToString(startPosition, 16).PadLeft(2, '0').ToUpper();
                //crc = 0;
            }
            return result.ToString().TrimEnd((char[])"\n\r".ToCharArray());

        }
        /// <summary>
        /// 计算有误 1.0.0.1升级后不用该方法
        /// </summary>
        /// <param name="data"></param>
        /// <param name="startPosition"></param>
        /// <param name="recordType"></param>
        /// <param name="lineLength"></param>
        /// <returns></returns>
        public static byte CalCRC(byte[] data, UInt32 startPosition, string recordType,byte lineLength = 0x10)
        {
            byte crc = 0;
            for (int i = 0; i < data.Length; i++)
            {
                crc += data[i];
            }
            if ((data.Length % lineLength) > 0)
                crc = (byte)(0xff - (byte)(crc + GetByteCRCByRecordType(data.Length, startPosition, lineLength, AddCrcrlen, recordType)));
            else
                crc = (byte)(0xff - (byte)(crc + GetByteCRCByRecordType(startPosition, lineLength, AddCrcrlen, recordType)));
            sbyte crctemp = (sbyte)crc;
            if (crctemp < 0)
            {
                crc = (byte)(sbyte.MaxValue - sbyte.MinValue + crctemp + 1);
            }
            return crc;
        }

        /// <summary>
        /// 计算CRC后续一截
        /// </summary>
        /// <param name="startPosition">起始地址</param>
        /// <param name="linelength">数据长度</param>
        /// <param name="addrCrcLength">地址+CRC地址长度</param>
        /// <returns></returns>
        public static byte GetByteCRCByRecordType(UInt32 startPosition, byte linelength, byte addrCrcLength,string recordType)
        {
            byte crcEnd = 0;
            if (recordType == "S2")
            {
                crcEnd = (byte)(linelength + addrCrcLength + startPosition + (startPosition >> 8) + (startPosition >> 16));
            }
            else if (recordType == "S3")
            {
                crcEnd = (byte)(linelength + addrCrcLength + startPosition + (startPosition >> 8) + (startPosition >> 16) + (startPosition >> 24));
            }
            return crcEnd;
        }

        public static byte GetByteCRCByRecordType(int datalength, UInt32 startPosition, byte linelength, byte addrCrcLength,string recordType)
        {
            byte crcEnd = 0;
            if (recordType == "S2")
            {
                crcEnd = (byte)(datalength % linelength + addrCrcLength + startPosition + (startPosition >> 8) + (startPosition >> 16));
            }
            else if (recordType == "S3")
            {
                crcEnd = (byte)(datalength % linelength + addrCrcLength + startPosition + (startPosition >> 8) + (startPosition >> 16) + (startPosition >> 24));
            }
            return crcEnd;
        }

        /// <summary>
        /// 拼接srec字符串 - record type+length+addr
        /// </summary>
        /// <param name="dataLength">总数据长度</param>
        /// <param name="RdPtr"></param>
        /// <param name="lineLength"></param>
        /// <param name="addrCrcLength"></param>
        /// <param name="addrLength"></param>
        /// <param name="startPosition"></param>
        /// <returns></returns>
        public static string GetLineHeaderByRecordType(int dataLength, int RdPtr, byte lineLength, byte addrCrcLength, byte addrLength, UInt32 startPosition,string recordType)
        {
            string header;
            if ((dataLength - RdPtr) >= lineLength)
            {
                header = recordType + Convert.ToString(lineLength + addrCrcLength, 16).PadLeft(2, '0').ToUpper() + Convert.ToString(startPosition, 16).PadLeft(addrLength * 2, '0').ToUpper();
            }
            else
            {
                header = recordType + Convert.ToString((dataLength % lineLength) + addrCrcLength, 16).PadLeft(2, '0').ToUpper() + Convert.ToString(startPosition, 16).PadLeft(addrLength * 2, '0').ToUpper();
            }

            return header;
        }

        public static void GetSType(string sType, out byte addrlen, out byte linelength, out string cmdEndStr)
        {
            cmdEndStr = "S9";
            if (sType == "S2")
            {
                addrlen = 0x3;
                linelength = 0x20;
                cmdEndStr = "S8";
            }
            else if (sType == "S3")
            {
                addrlen = 0x4;
                linelength = 0x10;
                cmdEndStr = "S7";
            }
            else if (sType == "S1")
            {
                addrlen = 0x2;
                linelength = 0x10;
            }
            else
            {
                throw new Exception("未知类型");
            }
        }

        /// <summary>
        /// 根据地址填充数据
        /// </summary>
        /// <param name="srecDatas">原数据</param>
        /// <param name="datalength">数据长度（从地址到CRC的数据长度），一行需要填充多少数据</param>
        /// <param name="fillData">填充的数据</param>
        /// <returns></returns>
        public static byte[] FillData(List<SrecData> srecDatas, int datalength,byte fillData = 0xFF)
        {
            List<byte> srecDataBytes = new List<byte>();

            srecDatas.Sort();
            //fill FF
            for (int i = 0; i < srecDatas.Count; i++)
            {
                srecDataBytes.AddRange(srecDatas[i].ToByte());
                if (i + 1 == srecDatas.Count)
                {
                    if (srecDatas[i].DataLength_OnlyData < datalength)
                    {
                        int count = srecDataBytes.Count % datalength;
                        for (int j = 0; j < (datalength - count); j++)
                        {
                            srecDataBytes.Add(fillData);
                        }
                    }
                    break;
                }
                int intervael = srecDatas[i + 1].Addr - srecDatas[i].Addr - srecDatas[i].DataLength_OnlyData;
                if (intervael > 0)
                {
                    for (int j = 0; j < intervael; j++)
                    {
                        srecDataBytes.Add(fillData);
                    }
                }
            }

            return srecDataBytes.ToArray();
        }
    }

}
