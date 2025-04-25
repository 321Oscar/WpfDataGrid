using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERad5TestGUI.Converters;

namespace ERad5TestGUI.UDS
{
    public class Result0x34
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data">06 74 40 00 00 04 02 AA</param>
        public Result0x34(byte[] data)
        {
            int length = data[2] >> 4;
            byte[] bytes = data.Skip(3).Take(length).ToArray();
            string lengthStr = string.Join("", bytes.Select(x => x.ToString("X").PadLeft(2,'0')));
            if (lengthStr.TryConvertToIntHex(out int lengthInt))
            {
                MaxNumberOfBlockLength = lengthInt;
            }
            else
            {
                throw new ArgumentException("0x34 解析错误");
            }
        }
        public int MaxNumberOfBlockLength { get; private set; }
    }

    [Serializable]
    public class UDSConfig
    {
        /// <summary>
        /// 正常报文超时时间
        /// </summary>
        public int NormalTimeout { get; set; }
        /// <summary>
        /// pending超时时间
        /// </summary>
        public int PendingTimeout { get; set; }

        public string SeedNKeyPath { get; set; } = @"Config\SeedNKey.dll";

        public List<UpgradeID> UpGradeIDs { get; set; }
        public List<DIDInfo> DIDInfos { get; set; }

        public List<DTCInfo> DTCInfos { get; set; }

        public List<SnapShot> SnapShots { get; set; }
        public List<UDSExtendedData> ExtendedDatas { get; set; }
    }

    public class ServerResult
    {
        public UDSResponse UDSResponse { get; set; }
        public string Message { get; set; }

        public int Progress { get; set; }
        public decimal ProgressWeights { get; set; }
        // public T Data { get; set; }
        public int Index { get; set; }

        public ServerResult(int index, decimal weights = 0)
        {
            Index = index;
            ProgressWeights = weights;
        }

        public override string ToString()
        {
            return $"{UDSResponse},{Message},{Progress}/100.";
        }
    }

    public class UDSException : Exception
    {
        public UDSException(string message) : base(message)
        {
        }
    }

    public enum DIDType
    {
        BCD,
        HEX,
        ASCII,
    }

    public enum UDSResponse
    {
        Init,
        Positive,
        Negative,
        Timeout,
        ParseError,
        Unknow,
        FlowControl,
        /// <summary>
        /// 有些服务，负响应也需要进行下一步，就赋值 pass
        /// </summary>
        Pass,
        SendFail,
        Cancel
    }

    public enum UDSServerCode
    {
        DiagnosticSessionControl = 0x10,
        ECUReset = 0x11,
        ClearDiagnosticInformation = 0x14,
        ReadDTCInfo = 0x19,

        SecurityAccess = 0x27,
        CommunicationControl = 0x28,

        ReadDataByIdentifier = 0x22,
        WriteDataByIdentifier = 0x2E,
        
        RountineControl = 0x31,

        RequestDownload = 0x34,
        RequestUpload = 0x35,

        DataTransfer = 0x36,
        RequestTransferExit = 0x37,

        TesterPresent = 0x3E,


        ControlDTCSetting = 0x85,
    }

    public enum UpgradeType
    {
        RMCU,//后驱
        FMCU,//前驱
        FPump,//FPump,//油泵
        RPump,//后驱动油泵
        GCU,//增程
        LCMCU
    }

    /// <summary>
    /// ID信息
    /// </summary>
    public class UpgradeID
    {
        
        public UpgradeID()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reqPhyID"></param>
        /// <param name="resPhyID">应答ID</param>
        /// <param name="reqFunID">默认为0x7df</param>
        public UpgradeID(uint reqPhyID, uint resPhyID, uint reqFunID = 0x7df, string upgradeType = "RMCU")
        {
            this.reqFunID = reqFunID;
            this.reqPhyID = reqPhyID;
            this.resPhyID = resPhyID;
            Type = upgradeType;
        }


        public string Type { get; set; }
         uint reqFunID;
         uint reqPhyID;
         uint resPhyID;
        public uint ReqFunID { get => reqFunID; set => reqFunID = value; }
        public uint ReqPhyID { get => reqPhyID; set => reqPhyID = value; }
        /// <summary>
        /// 应答ID
        /// </summary>
        public uint ResPhyID { get => resPhyID; set => resPhyID = value; }
        /// <summary>
        /// 是否是拓展帧
        /// </summary>
        public bool IDExtended { get; set; }

        public override string ToString()
        {
            return Type.ToString();
        }
    }
}
