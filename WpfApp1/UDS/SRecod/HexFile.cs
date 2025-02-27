using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERad5TestGUI.UDS.SRecord
{
    public class HexFile
    {
        public HexFile(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (StreamReader sw = new StreamReader(fs, Encoding.Default))
            {
                HexSections = new List<HexSection>();
                string newline;
                while ((newline = sw.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(newline))
                        continue;
                    string type = newline.Substring(7, 2);
                    if (type == "04")
                    {
                        HexSection hexSection = new HexSection(newline);
                        HexSections.Add(hexSection);
                    }
                    else if (type == "00")// != cmdEndStr)
                    {
                        HexLine hexData = new HexLine(newline);
                        HexSections[HexSections.Count - 1].Add(hexData);
                    }
                    else if(type == "01")
                    {
                        HexEnd = newline;
                    }
                }
            }
        }

        public string HexEnd { get; set; }
        public List<HexSection> HexSections { get; private set; }

        public void Add(uint startAddr, byte[] data)
        {

        }

        /// <summary>
        /// 输出文件
        /// </summary>
        /// <param name="outpuFile"></param>
        public void Output(string outpuFile, uint fillData = 0xff, bool addFill = false, bool isSort = true)
        {
            if (string.IsNullOrEmpty(outpuFile))
                outpuFile = $"{DateTime.Now:MMdd-HHmmss}.hex";
            if (outpuFile.IndexOf(".hex") < 0)
                outpuFile += ".hex";
            //if (isSort)
            //    srecDatas.Sort();

            //if (addFill)
            //{
            //    FillLastRecord(fillData);
            //}

            using (StreamWriter file = new StreamWriter(outpuFile, false))
            {
                //file.WriteLine(SrecHeader);
                foreach (var section in HexSections)
                {
                    file.WriteLine(section.ToString());
                    foreach (var hexLine in section.HexLines)
                    {
                        file.WriteLine(hexLine.ToString());
                    }
                }
                file.WriteLine(HexEnd);
            }
            Console.WriteLine($"Output File Path:[{outpuFile}].");
        }
    }

    public class HexSection : HexLine
    {
        public new uint Addr { get; set; }

        public HexSection(SrecData srec) : base(srec)
        {
            HexLines = new List<HexLine>();
            Type = "04";
        }

        public HexSection(string header) : base(header)
        {
            byte[] data = new byte[4];
            //var temp = Data;
            for (int i = 0; i < Data.Length; i++)
            {
                data[i] = Data[i];
            }
            Addr = BitConverter.ToUInt32(data.Reverse().ToArray(), 0);
            HexLines = new List<HexLine>();
        }

        public List<HexLine> HexLines { get; private set; }

        public void Add(HexLine hex)
        {
            HexLines.Add(hex);
        }
        public void Add(SrecData srec)
        {
            if (HexLines == null) HexLines = new List<HexLine>();
        }
       
        public override string ToString()
        {
            return base.ToString();
        }
    }

    public class HexLine
    {
        /// <summary>
        /// :10 C4B0 00 00000000000000000000000000000000 7C
        /// </summary>
        /// <param name="hexline"></param>
        public HexLine(string hexline) 
        {
            DataLength = byte.Parse(hexline.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
            Addr = ushort.Parse(hexline.Substring(3, 4), System.Globalization.NumberStyles.HexNumber);
            Type = hexline.Substring(7, 2);

            DataStr = hexline.Substring(9, DataLength * 2);
        }

        public HexLine(SrecData srec)
        {
            Addr = (ushort)srec.AddrUint;
            AddrStr = Addr.ToString("X");
            Data = srec.DataBytes;
            Type = "00";
            DataLength = (byte)Data.Length;
            RefreshCRC();
        }

        private ushort _addr;

        public string Head { get => ":"; }

        public ushort Addr { get => _addr; set => _addr = value; }
        public string AddrStr
        {
            get => _addr.ToString("X4");
            set => _addr = ushort.Parse(value, System.Globalization.NumberStyles.HexNumber);
        }
        public string Type { get; set; }
        private string _dataStr;
        public string DataStr {
            get
            {
                if (!string.IsNullOrEmpty(_dataStr))
                    return _dataStr;
                var builder = new StringBuilder();
                for (int i = 0; i < Data.Length; i++)
                {
                    builder.Append(Data[i].HexToString());

                }
                return builder.ToString();
            }
            set
            {
                if (value.Length % 2 > 0)
                {
                    _dataStr = value.ToString();
                    return;
                }
                _dataStr = string.Empty;
                Data = value.StringDataToBytes(2);
                DataLength = (byte)Data.Length;
                //this.DataLengthStr = (data.Length + this.AddrLength / 2 + 1).ToString("X");
                RefreshCRC();
            }
        }
        public byte[] Data { get; set; }

        public byte CRC { get; set; }

        public void RefreshCRC()
        {
            List<byte> crcdata = new List<byte>();
            crcdata.Add((byte)DataLength);

            byte[] addr = AddrStr.StringDataToBytes(2);
            crcdata.AddRange(addr);
            crcdata.Add(byte.Parse(Type));
            crcdata.AddRange(Data);

            var sum = crcdata.Sum(x => x);
            var CRCbyte = (byte)(~sum + 1);
            //byte CRCbyte = (byte)(256 - sum / 0x100);

            //var testData = new byte[] { 0x03, 0x00, 0x30, 0x00, 0x02, 0x33, 0x7a };
            //var sumtest = testData.Sum(x => x);
            //var crc = (byte)(~sumtest+1);

            CRC = CRCbyte;
        }

        public byte DataLength { get; set; }

        public override string ToString()
        {
            return $"{Head}{DataLength:X2}{AddrStr}{Type}{string.Join("", Data.Select(x => x.ToString("X2")))}{CRC:X2}";
        }
    }

    public class HexHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static int Main1(string[] args)
        {
            /**
            .\CalMerge.bat .\Debug\LP-ICHS040-AA-M0_BCM_APP.hex 0x3428c000 0x64000 .\Debug\CalData\CalData_2.bin LP-ICHS040-AA-M0_BCM_APP_CALMERGE2.hex
            Calibrate the area start address : 0x3428c000, Size : 0x64000
            Calibration data bin file : .\Debug\CalData\CalData_2.bin, Size : 0x357424
            Blank fill size: 0x52176
            The calibration data fills the start address : 0x342E3430
            Output hex file : LP-ICHS040-AA-M0_BCM_APP_CALMERGE2.hex
            Delete intermediate files
            */
            //输入 app hex文件，标定起始地址，标定区域大小，标定bin文件
            string appHexFile = args[0];//.\Debug\LP-ICHS040-AA-M0_BCM_APP.hex
            string calAddrHex = args[1];//0x3428c000
            string calSizeHex = args[2];//0x64000
            string calBinFile = args[3];//.\Debug\CalData\CalData_2.bin
            string outHexFile = args[4];//LP-ICHS040-AA-M0_BCM_APP_CALMERGE2.hex
            //获取bin文件大小
            int calBinFileSize = GetBinSize(calBinFile);//357424 0x57430

            int calSize = Hex2Dec(calSizeHex);//0x64000
            if (calBinFileSize > calSize)
            {
                //标定数据超出范围
            }

            //计算需要填充的尺寸:标定大小-bin文件大小
            int file_cal_area_size = calSize - calBinFileSize;//52176 0xCBD0

            //计算填充区域的地址
            int calAddrDec = Hex2Dec(calAddrHex); //0x3428c000
            int fileAddr = calAddrDec + calBinFileSize;//bin文件大小+标定地址
            string fileAddrHex = Dec2Hex(fileAddr);//0x342e3430

            //填充空白区域内容
            hexViewFill(0, fileAddrHex, file_cal_area_size, "filezero.hex");
            string calHexFileName = "CalData.hex";
            bin2Hex(calBinFile, calAddrHex, calHexFileName);

            //合并标定区域 完整的标定区域
            mergeHex(calHexFileName, "filezero.hex", $"Merge_{calHexFileName}");

            //hex文件清空标定区域内容
            delDataHex(appHexFile, calAddrHex, calSize, "AppNoCal.hex");

            //app的hex和标定的hex合并
            mergeHex("AppNoCal.hex", $"Merge_{calHexFileName}", outHexFile);

            return 0;
        }

        static void delDataHex(string fileName, string addr, int size, string output)
        {
            //调用hexview 删除hex中的数据
        }

        static void mergeHex(string hexFileName1, string hexFileName2, string output)
        {
            //调用hexview 将两个hex文件合并
        }

        static void bin2Hex(string fileName, string addr, string output)
        {
            //调用hexview 将bin文件转为hex
        }

        static void hexViewFill(byte fill, string addr, int size, string output)
        {
            //调用hexview 生成一个 hex文件
        }

        static int GetBinSize(string fileName)
        {
            return 0;
        }

        static int Hex2Dec(string hex)
        {
            return 0;
        }

        static string Dec2Hex(int dec)
        {
            return "0";
        }
    }
}
