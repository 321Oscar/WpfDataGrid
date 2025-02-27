using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ERad5TestGUI.Helpers;
using ERad5TestGUI.UDS.SRecord;

namespace ERad5TestGUI.UDS
{
    public enum OTAVer
    {
        Normal = 1,
        /// <summary>
        /// 分段，Flash和app分开发送
        /// </summary>
        Section,
        Security = 3,
        LC = 99,
    }

    public class BinDataSegment
    {
        public byte[] CheckSum;

        public byte[] Data;

        public byte[] DataStartAddress;

        public byte[] Length;

        public uint Length_uint;

        private byte[] _DataCRC;

        public BinDataSegment(byte[] alldata, int startidx)
        {
            CheckSum = alldata.Skip(startidx).Take(4).ToArray();
            DataStartAddress = alldata.Skip(startidx + 4).Take(4).ToArray();
            Length = alldata.Skip(startidx + 4 + 4).Take(4).ToArray();
        }

        public BinDataSegment()
        {
        }
        public byte[] DataCRC { get => _DataCRC; }

        public void CalDataCrc()
        {
            var CRC_Code = UDSHelper.Getcrc32(Data, Length_uint);
            _DataCRC = new byte[4];
            _DataCRC[0] = (byte)(CRC_Code >> (byte)24);
            _DataCRC[1] = (byte)(CRC_Code >> (byte)16);
            _DataCRC[2] = (byte)(CRC_Code >> (byte)8);
            _DataCRC[3] = (byte)(CRC_Code);
        }
    }

    public class BinTmpFile
    {

        public List<BinDataSegment> Segments;
        /// <summary>
        /// CRC占用的字节长度
        /// </summary>
        private const int CRCLENGTH = 4;

        /// <summary>
        /// info.json数据长度的字节长度
        /// </summary>
        private const int InfoLength_ByteLength = 4;

        /// <summary>
        /// LEAP的字节长度
        /// </summary>
        private const int MAGICLength = 4;

        /// <summary>
        /// 分段数的字节长度
        /// </summary>
        private const int NUMOFSegmentLength = 1;

        private static readonly List<string> binFile = new List<string>()
        {
            ".bin",
            ".tmp",
        };

        private static readonly List<string> srecFile = new List<string>()
        {
            ".srec",
            ".s19",
            ".s28",
            ".s37",
        };

        private string _filePath;
        public BinTmpFile() { Segments = new List<BinDataSegment>(); }

        public BinTmpFile(string filePath) : this()
        {
            this.FilePath = filePath;
            //BinaryReader br = new BinaryReader();

        }

        public string CompileDate { get; private set; }

        public string FilePath
        {
            get => _filePath; 
            set 
            { 
                _filePath = value;
            } 
        }

        public byte[] FlashDriver_CRC { get; private set; }

        public uint FlashDriver_Data_Lenth { get; private set; }

        public byte[] FlashDriver_Data_Lenth_bytes { get; private set; }

        public byte[] FlashDriver_DataBuffer { get; private set; }

        public byte[] FlashDriver_MEM_START_ADDR { get; private set; }

        public bool HasFlashDriver { get; private set; }

        public bool HasSecurityData { get => OtaVer == OTAVer.Security; }

        public string Info { get; private set; }

        public OTAVer OtaVer { get; private set; }

        public byte[] Security_Data { get; private set; }

        public int SecurityInfoLength { get; private set; }

        /// <summary>
        /// 分段数
        /// </summary>
        public int SegmentsNum { get; private set; }

        public string SoftWareVersion { get; private set; }

        /// <summary>
        /// bin/tmp文件总长度
        /// </summary>
        public uint Tmp_Data_Lenth { get; private set; }
        /// <summary>
        /// tmp文件数据
        /// </summary>
        public byte[] Tmp_DataBuffer { get; private set; }
        public string UpdateDate { get => DateTime.Now.ToString("yyyyMMdd"); }
        public static bool IsBinFile(string fileExt) => binFile.Contains(fileExt);

        public static bool IsSrecFile(string fileExt) => srecFile.Contains(fileExt);

        public bool AnalysisBinFile(string path, out string failReason)
        {
            failReason = String.Empty;
            try
            {
                FilePath = path;
                string filePath = Path.GetDirectoryName(path);
                string fileFullPath = path;
                if (Path.GetExtension(path) == ".bin")
                {
                    if (UDSHelper.DecryptBin2Tmp(path, out string res))
                    {
                        fileFullPath = Path.Combine(AppContext.BaseDirectory, res);
                    }
                    else
                    {
                        failReason = "解密失败";
                        return false;
                    }
                }

                using (FileStream file = new FileStream(fileFullPath, FileMode.Open))
                {
                    Tmp_Data_Lenth = (uint)file.Length;
                    if (Tmp_Data_Lenth == 0)
                    {
                        return false;
                    }

                    Tmp_DataBuffer = new byte[Tmp_Data_Lenth];
                    _ = file.Read(Tmp_DataBuffer, 0, (int)Tmp_Data_Lenth);
                    file.Close();
                }

                if (Path.GetExtension(path) == ".bin")
                {
                    File.Delete(fileFullPath);
                }
                byte[] jsonLengthByte = Tmp_DataBuffer.Skip(MAGICLength)
                    .Take(InfoLength_ByteLength)
                    .Reverse()
                    .ToArray();
                int json_length = BitConverter.ToInt32(jsonLengthByte, 0);

                var _Info = Tmp_DataBuffer.Skip(MAGICLength + InfoLength_ByteLength)
                    .Take(json_length)
                    .ToArray();
                int headerDataLength = json_length + MAGICLength + InfoLength_ByteLength;

                Info = System.Text.Encoding.ASCII.GetString(_Info);


                int infoIdxSta = Info.IndexOf("OtaVer\":") + "OtaVer\":".Length;
                int infoIdxEnd = Info.Substring(infoIdxSta).IndexOf(",");
                string otaver = Info.Substring(infoIdxSta, infoIdxEnd).Trim();
                if (infoIdxSta > 8)
                {
                    OtaVer = (OTAVer)(int.Parse(otaver));
                }
                else
                {
                    OtaVer = OTAVer.Normal;
                }

                if (OtaVer == OTAVer.Security)
                {
                    infoIdxSta = Info.IndexOf("SignInfoLen\":") + "SignInfoLen\":".Length;
                    infoIdxEnd = Info.Substring(infoIdxSta).IndexOf(",");
                    string securityLength = Info.Substring(infoIdxSta, infoIdxEnd).Trim();
                    //SecurityInfoLength = );
                    if (int.TryParse(securityLength, out int len))
                    {
                        SecurityInfoLength = len;
                    }
                    else
                    {
                        failReason = "Get SecurityInfoLength Fail";
                        return false;
                    }
                }

                infoIdxSta = Info.IndexOf("FlashDriverNo\":") + "FlashDriverNo\":".Length;
                infoIdxEnd = Info.Substring(infoIdxSta).IndexOf(",");
                string fd = Info.Substring(infoIdxSta, infoIdxEnd).Trim();
                HasFlashDriver = fd == "1";

                infoIdxSta = Info.IndexOf("SWV\":") + "SWV\":".Length;
                infoIdxEnd = Info.Substring(infoIdxSta).IndexOf(",");
                SoftWareVersion = Info.Substring(infoIdxSta, infoIdxEnd).Replace('"', ' ').Trim();

                infoIdxSta = Info.IndexOf("CompileDate\":") + "CompileDate\":".Length;
                infoIdxEnd = Info.Substring(infoIdxSta).LastIndexOf('"');
                CompileDate = Info.Substring(infoIdxSta, infoIdxEnd).Replace('"', ' ').Trim();

                SegmentsNum = Tmp_DataBuffer[headerDataLength + NUMOFSegmentLength - 1];

                //flash
                //if (HasFlashDriver)
                //{
                //    Idx_Flashdriver = (uint)(skipDataLength + NUMOFSegmentLength + CRCLENGTH);
                //}
                int segmentInfoLength = 12 * SegmentsNum;
                uint takedSegmentData = 0;
                for (int i = 0; i < SegmentsNum; i++)
                {
                    int segment_crc_idx = headerDataLength + NUMOFSegmentLength + i * 12;
                    int segment_startaddr_idx = headerDataLength + NUMOFSegmentLength + i * 12 + 4;
                    int segment_length_idx = headerDataLength + NUMOFSegmentLength + i * 12 + 8;
                    BinDataSegment segment = new BinDataSegment();

                    segment.CheckSum = new byte[4];
                    segment.DataStartAddress = new byte[4];
                    segment.Length = new byte[4];
                    for (int j = 0; j < 4; j++)
                    {
                        segment.CheckSum[j] = Tmp_DataBuffer[segment_crc_idx + j];
                        segment.DataStartAddress[j] = Tmp_DataBuffer[segment_startaddr_idx + j];
                        segment.Length[j] = Tmp_DataBuffer[segment_length_idx + j];
                        segment.Length_uint = (segment.Length_uint << 8) + Tmp_DataBuffer[segment_length_idx + j];
                    }
                    segment.Data = new byte[segment.Length_uint];

                    for (int m = 0; m < segment.Length_uint; m++)
                    {
                        segment.Data[m] = Tmp_DataBuffer[headerDataLength + NUMOFSegmentLength + segmentInfoLength + takedSegmentData + m];
                    }

                    takedSegmentData += segment.Length_uint;
                    Segments.Add(segment);
                }

                if (HasFlashDriver)
                {
                    var flashSegment = Segments[0];
                    //flashSegment.CalDataCrc();
                    FlashDriver_MEM_START_ADDR = flashSegment.DataStartAddress;
                    FlashDriver_DataBuffer = flashSegment.Data;
                    FlashDriver_Data_Lenth = flashSegment.Length_uint;
                    FlashDriver_Data_Lenth_bytes = flashSegment.Length;
                    FlashDriver_CRC = flashSegment.CheckSum;
                    Segments.RemoveAt(0);
                }
                //证书
                if (OtaVer == OTAVer.Security)
                    Load_Security_Data();
                //#if DEBUG
                //                //Output segments
                //                Task.Run(new Action(() =>
                //                {
                //                    foreach (var segment1 in Segments)
                //                    {
                //                        SrecFile s = new SrecFile("S3", datalength: 0x20);
                //                        var addrByte = segment1.DataStartAddress;
                //                        addrByte = addrByte.Reverse().ToArray();
                //                        uint addr = BitConverter.ToUInt32(addrByte, 0);
                //                        s.Add(segment1.Data, addr);
                //                        s.Output($"temp-{addr:X}.srec");
                //                    }
                //                }));
                //#endif

                return true;
            }
            catch (Exception e)
            {
                failReason = (e.Message);
                return false;
            }
        }

        public bool AnalysisSrecFile(string path, out string failReason)
        {
            failReason = string.Empty;
            try
            {
                SrecFile s = new SrecFile(path);
                foreach (var addrAndBytes in s.AddrDataUnsort)
                {
                    BinDataSegment segment = new BinDataSegment();
                    segment.DataStartAddress = BitConverter.GetBytes(addrAndBytes.Key);
                    Array.Reverse(segment.DataStartAddress);
                    segment.Data = addrAndBytes.Value.ToArray();
                    segment.Length_uint = (uint)addrAndBytes.Value.Count;
                    segment.Length = BitConverter.GetBytes(addrAndBytes.Value.Count);
                    Array.Reverse(segment.Length);
                    segment.CalDataCrc();
                    Segments.Add(segment);
                }
            }
            catch (Exception er)
            {
                failReason = er.Message;
                return false;
            }

            if (Segments.Count < 2)
            {
                failReason = "Loss FlashDriver Data.please add FlashDriver Data";
                return false;
            }

            OtaVer = OTAVer.LC;
            return true;
        }

        public bool ParseBinOrSrecFile(string path, out string failReason)
        {
            failReason = String.Empty;
            string fileExtension = Path.GetExtension(path);
            if (IsBinFile(fileExtension))
            {
                if (!AnalysisBinFile(path, out string fail))
                {
                    failReason = ($"Error 文件解析失败{fail}！【{path}】");
                    return false;
                }
            }
            else if (IsSrecFile(fileExtension))
            {
                if (!AnalysisSrecFile(path, out string fail))
                {
                    failReason = ($"Error 文件解析失败{fail}！【{path}】");
                    return false;
                }
            }
            else
            {
                failReason = "";
                return false;
            }

            return true;
        }
        private void Load_Security_Data()
        {
            Security_Data = new byte[SecurityInfoLength];
            for (int i = 0; i < SecurityInfoLength; i++)
            {
                Security_Data[i] = (Tmp_DataBuffer[Tmp_Data_Lenth - SecurityInfoLength + i]);
                //Verify_File_Legitimacy_Buffer[i] = Tmp_DataBuffer[];
            }
        }
    }
    [Serializable]
    public class DTCInfo
    {
        public string Comment { get; set; }
        public string Key { get; set; }
    }

    public class DTCModel
    {
        public DTCModel(byte[] data)
        {
            DTCData = data;
            SnapDIDs = new ObservableCollection<SnapSubDIDInfo>();
            ExpandInfos = new ObservableCollection<SnapSubDIDInfo>();
        }

        public string Comment { get; set; }
        public byte[] DTCData { get; private set; }
        public byte DTCHighByte { get => DTCData[0]; }
        public byte DTCLowByte { get => DTCData[2]; }
        public byte DTCMiddleByte { get => DTCData[1]; }
        public ObservableCollection<SnapSubDIDInfo> ExpandInfos { get; set; }
        public string Key { get => this.ToString(); }
        public ObservableCollection<SnapSubDIDInfo> SnapDIDs { get; set; }
        public byte StatusOfDTC { get => DTCData[3]; }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return $"P{DTCHighByte:X2}{DTCMiddleByte:X2}{DTCLowByte:X2}";
        }
    }
}
