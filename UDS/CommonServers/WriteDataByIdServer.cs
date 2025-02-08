using System;
using System.Collections.Generic;
using System.Linq;
using WpfApp1.Devices;
using WpfApp1.Services;

namespace WpfApp1.UDS
{
    public class WriteDataByIdServer : UDSServerBase
    {
        public int MAXDataLength { get => MsgMaxLength - 5; }
        private DIDInfo _didInfo;
        public DIDInfo DIDInfo
        {
            get => _didInfo;
            set
            {
                _didInfo = value;
                this.ServerName = $"WriteDID {_didInfo.Name} 0x{_didInfo.DID:X4} [{_didInfo.Data}]";
            }
        }
        public override UDSServerCode CurrentUDSFunction { get; protected set; } = UDSServerCode.WriteDataByIdentifier;
        /// <summary>
        /// 写入的数据涉及当前日期，写入时更新
        /// </summary>
        public Action ModifyDate;

        public WriteDataByIdServer(uint slaver, uint master, IDevice device, ILogService logService) : base(slaver, master, device, logService)
        {
            ServerName = "WriteDID";

            ModifyErrCode(0x31, "not support in this server OR readonly.");
        }

        public override byte[] BuildFrame()
        {
            //var didData = DataByte();
            List<byte> data = new List<byte>();
            //data.AddRange(BitConverter.GetBytes(DIDInfo.DID).Reverse());
            //data.AddRange(didData);

            //SendDatas = data.ToArray();

            //return base.BuildFrame();
            SendDatas = DataByte();
            if (DIDInfo.Length > MAXDataLength + 1)
            {
                //发首帧...
                int length = MAXDataLength + DIDInfo.Length;
                byte databyte0 = (byte)(0x10 | (length >> MsgMaxLength));
                data.Add(databyte0);
                byte databyte1 = (byte)(length & 0xff);
                data.Add(databyte1);

                data.Add((byte)CurrentUDSFunction);
                //add did
                data.AddRange(BitConverter.GetBytes(DIDInfo.DID).Reverse());
                //add data
                data.AddRange(SendDatas.Take(MAXDataLength));
                int sendCount = (int)Math.Ceiling((double)(SendDatas.Length - MAXDataLength) / ConsectiveFrameMaxLength);
                //int remainBytes = (datas.Length - MAXDataLength) % 7;
                for (int i = 0; i < sendCount; i++)
                {
                    data.Add((byte)(0x20 + i + 1));
                    if(SendDatas.Skip(MAXDataLength + i * ConsectiveFrameMaxLength).Count() < ConsectiveFrameMaxLength)
                    {
                        data.AddRange(SendDatas.Skip(MAXDataLength + i * ConsectiveFrameMaxLength));
                    }
                    else
                    {
                        data.AddRange(SendDatas.Skip(MAXDataLength + i * ConsectiveFrameMaxLength).Take(ConsectiveFrameMaxLength));
                    }
                }
            }
            else
            {
                //add func
                data.Add((byte)CurrentUDSFunction);
                //add did
                data.AddRange(BitConverter.GetBytes(DIDInfo.DID).Reverse());
                //add data
                data.AddRange(SendDatas);
                if (data.Count > 7)
                {
                    data.Insert(0, (byte)data.Count);
                    data.Insert(0, 0);
                }
                else
                    data.Insert(0, (byte)data.Count);
            }


            return FillFrame(data);
        }

        private byte[] DataByte()
        {
            if (ModifyDate != null)
            {
                ModifyDate?.Invoke();
                ServerName = $"WriteDID {_didInfo.Name} 0x{_didInfo.DID:X4} [{_didInfo.Data}]";
            }
           
            return DIDInfo.DataByte();
        }
    }
}
