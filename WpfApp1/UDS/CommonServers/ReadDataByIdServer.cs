using System;
using System.Linq;
using ERad5TestGUI.Devices;
using ERad5TestGUI.Services;

namespace ERad5TestGUI.UDS
{
    public class ReadDataByIdServer : UDSServerBase
    {
        public override UDSServerCode CurrentUDSFunction { get; protected set; } = UDSServerCode.ReadDataByIdentifier;
        private DIDInfo _didInfo;
        public DIDInfo DIDInfo { get => _didInfo; set { _didInfo = value; this.Name += $" {_didInfo.Name} 0x{_didInfo.DID:X4}"; } }

        public ReadDataByIdServer(uint slaver, uint master, IDevice device, ILogService logService) : base(slaver, master, device, logService)
        {
            ServerName = "ReadDID";
        }

        public override byte[] BuildFrame()
        {
            SendDatas = BitConverter.GetBytes(DIDInfo.DID);
            SendDatas = SendDatas.Reverse().ToArray();
            return base.BuildFrame();
        }

        public override ServerResult Run(object param = null)
        {
            base.Run(param);
            if (ServerResult.UDSResponse != UDSResponse.Positive && ServerResult.UDSResponse != UDSResponse.SendFail)
                Result = UDSResponse.Pass;
            return ServerResult;
        }

        public override void ParseData(byte[] data)
        {
            DIDInfo.Byte2String(data.Skip(3).ToArray());

            Result = UDSResponse.Positive;
            ResultMsg = $"{CurrentStep}：{DIDInfo.Data}";
            ProgressInt = 100;
            base.ParseData(data);
        }
       

        public override bool ParseResponse(byte[] receive)
        {
            return base.ParseResponse(receive);
        }

        protected override void ParsePositiveSingleFrame(byte[] receivedata)
        {
            //if(did)
            if (receivedata.Length > 8)
            {
                int length = receivedata[1];
                _buffer.AddRange(receivedata.ToList().Skip(2).Take(length));
            }
            else
            {
                int length = receivedata[0];
                _buffer.AddRange(receivedata.ToList().Skip(1).Take(length));
            }

            ParseData(_buffer.ToArray());
            //base.ParsePositiveSingleFrame(receivedata);
        }

        protected override void ParseFirstFrmame(byte[] receive)
        {
            base.ParseFirstFrmame(receive);
            //int length_ff = recieveByteLength = ((receive[0] & 0x0f) * 0x100) + (receive[1] & 0xff);//数据长度
            ////_receiveDataCount = (length_ff - 6) / 7 + ((((length_ff - 6) % 7) > 0) ? 1 : 0);
            ////_receiveDataCount = length_ff - 3;
            //_buffer.AddRange(receive.ToList().Skip(2));

            //SendFrame = new CANSendFrame((int)PhyID_Req, UDSHelper.Frame0x30);
           
            //CurrentStatue = ServerStatus.WaitReceive;
        }

        protected override void ParseContinueFrame(byte[] receive)
        {
            base.ParseContinueFrame(receive);
            //int index = receive[0] & 0x0f;
            //int leftcount = recieveByteLength - _buffer.Count;

            //if(leftcount <= 7)
            //{
            //    _buffer.AddRange(receive.ToList().Skip(1).Take(leftcount));
            //}
            //else
            //{
            //    _buffer.AddRange(receive.ToList().Skip(1));
            //}

            //if (_buffer.Count == recieveByteLength)
            //{
            //    ParseData(_buffer.ToArray());
            //    CurrentStatue = ServerStatus.Done;
            //}
            //else
            //{
            //    //_buffer.AddRange(receive.ToList().Skip(1));
            //    CurrentStatue = ServerStatus.WaitReceive;
            //}
        }

        public override void ResetResult()
        {
            base.ResetResult();
            this._buffer.Clear();
        }
    }
}
