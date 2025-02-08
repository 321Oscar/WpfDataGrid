using System.Collections.Generic;
using System.Linq;
using WpfApp1.Devices;
using WpfApp1.Services;

namespace WpfApp1.UDS
{
    public class ReadDTCInfo<T> : UDSServerBase where T : class
    {
        //private readonly List<byte> _buffer = new List<byte>();
       // private int _receiveDataCount;
       
        public byte DTCStatusMask { get; set; }
        /// <summary>
        /// 支持的DTC掩码
        /// </summary>
        public byte DTCStatusAvailableMask { get; set; }

        public uint DTCCount { get; private set; }
        //public new string CurrentStep { get => $"{ServerName} {SubFuncCode:X2}"; }
      
        public IUDSSubFuncData<T> SubFunc { get; set; }
        public ReadDTCInfo(uint slaver, uint master, IDevice device, ILogService logService) 
            : base(slaver, master, device, logService)
        {
            ServerName = "ReadDTC";
        }

        public override UDSServerCode CurrentUDSFunction { get; protected set; } = UDSServerCode.ReadDTCInfo;

        public override byte[] BuildFrame()
        {
            switch (SubFuncCode.Value)
            {
                case 0x01:
                case 0x02:
                    SendDatas = new byte[] { DTCStatusMask};
                    break;
                case 0x04:
                    //SendDatas = new byte[] { SnapDTC.DTCData[0], SnapDTC.DTCData[1], SnapDTC.DTCData[2], Snapshot.Index };
                    SendDatas = ((ResDTC04)SubFunc).GetBytes();
                    break;
                case 0x06:
                    SendDatas = ((ResDTC06)SubFunc).GetBytes();
                    break;
                default:
                    break;
            }

            return base.BuildFrame();
        }

        public override void ParseData(byte[] data)
        {
            data = data.Skip(2).ToArray();
            if (data.Length > 0)
                switch (SubFuncCode.Value)
                {
                    case 0x01:
                        DTCStatusAvailableMask = (byte)data[0];
                        string[] zerobyte = new string[] { data[2].ToString("x2"), data[3].ToString("x2") };
                        DTCCount = uint.Parse(zerobyte[0] + zerobyte[1], System.Globalization.NumberStyles.HexNumber);
                        ResultMsg = $"{CurrentStep}：avaMask:{DTCStatusAvailableMask:X} ,DTCCount:{DTCCount}";
                        Result = UDSResponse.Positive;
                        break;
                    case 0x02:
                    case 0x04://快照
                    case 0x06://拓展数据
                        //DTCStatusAvailableMask = (byte)data[0];
                        //if (((data.Length - 1) % 4) != 0)
                        //{
                        //    Result = UDSResponse.ParseError;
                        //    ResultMsg = $"{CurrentStep}：Length Error";
                        //}
                        //else
                        //{
                        //    int dtcCount = (data.Length - 1) / 4;
                        //    for (int i = 0; i < dtcCount; i++)
                        //    {
                        //        DTCModel dtc = new DTCModel(data.Skip(1 + i * 4).Take(4).ToArray());
                        //        dTCs.Add(dtc);
                        //        //ResultMsg += $"{dtc} \r";
                        //    }
                        //    ResultMsg = $"{CurrentStep}：Count:{dtcCount} End";
                        //}
                        SubFunc.ParseByte(data);
                        ResultMsg = $"{CurrentStep} {SubFunc.ResultMsg}";
                        Result = SubFunc.Result;
                        break;
                   
                        //int takedCount = 0;
                        //foreach (var snapdid in Snapshot.SnapDIDInfos)
                        //{
                        //    snapdid.ParseByte(data.Skip(8 + takedCount).Take(snapdid.Length + 2).ToArray());
                        //    takedCount += snapdid.Length + 2;
                        //}
                        //ResultMsg = $"{CurrentStep}：End";
                   
                    default:
                        break;
                }
            else
            {
                ResultMsg = $"{CurrentStep} {END}.But No Data!!";
                //Result = UDSResponse.Positive;
            }

            ProgressInt = 100;
            base.ParseData(data);
        }

        protected override void ParsePositiveSingleFrame(byte[] receivedata)
        {
            switch (SubFuncCode.Value)
            {
                case 0x01:
                case 0x02:
                    _buffer.AddRange(receivedata.ToList().Skip(3).Take(recieveByteLength - 3));
                    break;
                
                    //_buffer.AddRange(receivedata.ToList().Skip(4));
                    //break;
                case 0x04:
                    if (receivedata[7] != ((ResDTC04)SubFunc).Values[0].Index)
                    {
                        Result = UDSResponse.Positive;
                        ResultMsg = $"{CurrentStep} Error Read Snap {((ResDTC04)SubFunc).Values[0].Index}:{receivedata[7]:X2}";
                    }
                    break;
                case 0x06:
                    if (receivedata[7] != ((ResDTC06)SubFunc).Values[0].RecordNum)
                    {
                        Result = UDSResponse.Positive;
                        ResultMsg = $"{CurrentStep} Error Read Snap {((ResDTC06)SubFunc).Values[0].RecordNum}:{receivedata[7]:X2}";
                    }
                    break;
                default:
                    break;
            }

            ParseData(_buffer.ToArray());
            //base.ParsePositiveSingleFrame(receivedata);
        }

        protected override void ParseFirstFrmame(byte[] receive)
        {
            base.ParseFirstFrmame(receive);
            //int length_ff = recieveByteLength = ((receive[0] & 0x0f) * 0x100) + (receive[1] & 0xff);//数据长度
            //_receiveDataCount = (length_ff - 6) / 7 + ((((length_ff - 6) % 7) > 0) ? 1 : 0) % 0x10;
            //_buffer.AddRange(receive.ToList().Skip(4));

            //SendFrame = new CANSendFrame((int)PhyID_Req, UDSHelper.Frame0x30);

            //CurrentStatue = ServerStatus.WaitReceive;
        }

        protected override void ParseContinueFrame(byte[] receive)
        {
            base.ParseContinueFrame(receive);
            //int index = receive[0] & 0x0f;
            //int leftcount = recieveByteLength - _buffer.Count - 2;
            //if (index == _receiveDataCount && leftcount <= 7)
            //{
            //    _buffer.AddRange(receive.ToList().Skip(1).Take(leftcount));
            //    ParseData(_buffer.ToArray());
            //    CurrentStatue = ServerStatus.Done;
            //}
            //else
            //{
            //    _buffer.AddRange(receive.ToList().Skip(1));
            //    CurrentStatue = ServerStatus.WaitReceive;
            //}
        }
    }
    /// <summary>
    /// ReadDTC 0x19 0x02 响应数据解析方式
    /// </summary>
    public class ResDTC02 : IUDSSubFuncData<DTCModel>
    {
        public byte DTCStatusAvailableMask { get; private set; }
        public List<DTCModel> Values { get; set; }
        public UDSResponse Result { get; set; }
        public string ResultMsg { get; set; }


        public ResDTC02()
        {
            Values = new List<DTCModel>();
        }

        public void ParseByte(byte[] data)
        {
            DTCStatusAvailableMask = (byte)data[0];
            //if (((data.Length - 1) % 4) != 0)
            //{
            //    Result = UDSResponse.ParseError;
            //    ResultMsg = $"Length Error";
            //}
            //else
            //{
                int dtcCount = (data.Length - 1) / 4;
                for (int i = 0; i < dtcCount; i++)
                {
                    DTCModel dtc = new DTCModel(data.Skip(1 + i * 4).Take(4).ToArray());
                    Values.Add(dtc);
                    //ResultMsg += $"{dtc} \r";
                }
                Result = UDSResponse.Positive;
                ResultMsg = $"Count:{dtcCount} {UDSServerAbstract.END}";
            //}
        }
    }
    /// <summary>
    /// ReadDTC 0x19 0x04 响应数据解析方式
    /// </summary>
    public class ResDTC04 : IUDSSubFuncData<SnapShot>
    {
        public DTCModel DTCModel { get; private set; }
        public byte StatusOfDTC { get; private set; }
        public byte DTCSnaoshotRecordNumber { get; private set; }
        public byte DTCSnaoshotRecordNumberOfIdentifiers { get; private set; }
        public List<SnapShot> Values { get; set; }
        public UDSResponse Result { get; set; }
        public string ResultMsg { get; set; }

        public ResDTC04(DTCModel dTC, SnapShot snapShot)
        {
            DTCModel = dTC;
            Values = new List<SnapShot>() { snapShot };
        }
        public byte[] GetBytes()
        {
            return new byte[] { DTCModel.DTCHighByte, DTCModel.DTCMiddleByte, DTCModel.DTCLowByte, Values[0].Index };
        }

        public void ParseByte(byte[] data)
        {
            DTCModel = new DTCModel(data.Take(4).ToArray());
            StatusOfDTC = data[3];
            DTCSnaoshotRecordNumber = data[4];
            DTCSnaoshotRecordNumberOfIdentifiers = data[5];
            int takedCount = 0;
            foreach (var snapdid in Values[0].SnapDIDInfos)
            {
                snapdid.ParseByte(data.Skip(8 + takedCount).Take(snapdid.Length + 2).ToArray());
                takedCount += snapdid.Length + 2;
            }
            Result = UDSResponse.Positive;
            ResultMsg = $"{DTCModel} {UDSServerAbstract.END}";
        }

        public override string ToString()
        {
            return DTCModel.ToString();
        }
    }

    /// <summary>
    /// ReadDTC 0x19 0x04 响应数据解析方式
    /// </summary>
    public class ResDTC06 : IUDSSubFuncData<UDSExtendedData>
    {
        public DTCModel DTCModel { get; private set; }
        public byte StatusOfDTC { get; private set; }
        public byte DTCExtendedRecordNumber { get; private set; }
        //public byte DTCSnaoshotRecordNumberOfIdentifiers { get; private set; }
        public List<UDSExtendedData> Values { get; set; }
        public UDSResponse Result { get; set; }
        public string ResultMsg { get; set; }

        public ResDTC06(DTCModel dTC, UDSExtendedData ExtendedData)
        {
            DTCModel = dTC;
            Values = new List<UDSExtendedData>() { ExtendedData };
        }
        public byte[] GetBytes()
        {
            return new byte[] { DTCModel.DTCHighByte, DTCModel.DTCMiddleByte, DTCModel.DTCLowByte, Values[0].RecordNum };
        }

        public void ParseByte(byte[] data)
        {
            DTCModel = new DTCModel(data.Take(4).ToArray());
            StatusOfDTC = data[3];
            DTCExtendedRecordNumber = data[4];
            //DTCSnaoshotRecordNumberOfIdentifiers = data[5];
            int takedCount = 0;
            foreach (var snapdid in Values[0].SnapDIDInfos)
            {
                snapdid.Parse = new System.Action<byte[], SnapSubDIDInfo>((b, s) =>
                {
                    s.StrVal = b[0].ToString();
                });
                snapdid.ParseByte(data.Skip(5 + takedCount).Take(snapdid.Length).ToArray());
                takedCount += snapdid.Length;
            }
            Result = UDSResponse.Positive;
            ResultMsg = $"{DTCModel} {UDSServerAbstract.END}";
        }

        public override string ToString()
        {
            return DTCModel.ToString();
        }
    }

    /// <summary>
    /// 不同子功能的不同解析方式
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IUDSSubFuncData<T> where T : class
    {
        List<T> Values { get; set; }
        UDSResponse Result { get;  set; }
        string ResultMsg { get;  set; }

        void ParseByte(byte[] data);
    }
}