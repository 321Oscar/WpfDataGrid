﻿using System;
using System.Collections.Generic;
using System.Linq;
using ERad5TestGUI.Converters;
using ERad5TestGUI.Devices;
using ERad5TestGUI.Helpers;
using ERad5TestGUI.Services;

namespace ERad5TestGUI.UDS
{
    public class DataTransferServer : UDSServerBase
    {
        /// <summary>
        /// 首帧发送的最长数据长度
        /// <para>can 2.0: 8 - 2(length) - 1(serverid) - 1(blockSeq) = 4</para>
        /// <para>can 2.0 (发送数据长度 > 4095): 8 - 2 - 4(length) - 1(serverid) - 1(blockSeq) = 0</para>
        /// <para>can fd : 64 - 2(length) - 1(serverid) - 1(blockSeq) = 60</para>
        /// <para>can fd (发送数据长度 > 4095): 64 - 2 - 4(length) - 1(serverid) - 1(blockSeq) = 56</para>
        /// </summary>
        public int FirstFrameMaxByteCount
        {
            get
            {
                return _firstFrameDataCount;
            }
            set
            {
                _firstFrameDataCount = value;
            }
        }
        private int _firstFrameDataCount;
        private int _sendedIndex = 1;
        private int _bSCount = 0;
        public override UDSServerCode CurrentUDSFunction { get; protected set; } = UDSServerCode.DataTransfer;
        /// <summary>
        /// 一个block 发送的最多数据,需要减/加上0x36, block sqe两个byte
        /// </summary>
        public int MaxNumOfBlockLength { get => ReadLen + 2; set => ReadLen = value - 2; }
        /// <summary>
        /// 一次发送的数据长度
        /// </summary>
        public int ReadLen { get; set; } = 1024;
        /// <summary>
        /// start from 1
        /// </summary>
        public int SendedIndex
        {
            get => _sendedIndex;
            set
            {
                _sendedIndex = value;
                DebugLogger($"{CurrentStep} sendedindex update {_sendedIndex}");
                _bSCount = 0;
            }
        }

        public DataTransferServer(uint slaver, uint master, IDevice device, ILogService logService, string custom = "") : base(slaver, master, device, logService)
        {
            ServerName = $"DataTransfer {custom}";
            FirstFrameMaxByteCount = MsgMaxLength - 2;
        }

        public override ServerResult Run(object param = null)
        {
            if (param != null && param is Result0x34 result0X34)
            {
                this.MaxNumOfBlockLength = result0X34.MaxNumberOfBlockLength;
                DebugLogger($"{CurrentStep} update Block Length {MaxNumOfBlockLength}");
            }
            return base.Run(param);
        }

        public override void Reset()
        {
            base.Reset();
            this.SendedIndex = 1;
            Offset = 0;
            RemainingData = 0;
        }

        public override byte[] BuildFrame()
        {
            if (SendDatas.Length > FirstFrameMaxByteCount + 1)
            {
                return BuildFirstFrameByOffset();
            }
            else if (SendDatas.Length > 0)
            {
                if (Offset == 0)
                    RemainingData = SendDatas.Length;
                if (RemainingData == 0)
                    return null;

                var newSendData = SendDatas.ToList();
                newSendData.Insert(0, 0x01);
                SendDatas = newSendData.ToArray();
                Offset += newSendData.Count;
                RemainingData = 0;
                return base.BuildFrame();
            }
            else
            {
                return FillFrame(new List<byte> { 0x01, 0x36 });
            }
        }

        public override bool ParseResponse(byte[] receive)
        {
            return base.ParseResponse(receive);
        }

        public override void ParseData(byte[] data)
        {
            //int responseBlockNum = data[2];
            var senddata = BuildFrame();
            //Console.WriteLine($"{DateTime.Now:HH:mm:ss fff} Build Frame Done.");
            if (senddata != null)
            {
                SendFrames(senddata);
            }
            else
            {
                CurrentStatue = ServerStatus.Done;
            }
        }

        int _offset = 0;
        int _remainingData;
        /// <summary>
        /// 已发送的bytecount
        /// </summary>
        public int Offset
        {
            get => _offset;
            set
            {
                _offset = value;
                //DebugLogger($"Offset Change to {_offset}");
            }
        }
        /// <summary>
        /// 剩余发送的数据长度
        /// </summary>
        public int RemainingData { get => _remainingData; set => _remainingData = value; }
        /// <summary>
        /// 当前发送的数据长度
        /// </summary>
        public int PayloadSize { get => _payloadSize; set => _payloadSize = value; }


        /// <summary>
        /// 组首帧
        /// </summary>
        /// <returns></returns>
        private byte[] BuildFirstFrameByOffset()
        {
            if (Offset == 0)
                RemainingData = SendDatas.Length;
            if (RemainingData == 0)
                return null;

            var result = new List<byte>();
            //14 02 36 01 + data
            /**
             * byte 0 + byte 1 : payload Size,remainingData.length + 2
             * byte 2 : server id
             * byte 3 : block seq
             * byte 4-7: data
             */
            /** Test
             * send 2050 bytes data
             * step 1：offset = 0;remaindata = 2050
             * payloadSize = 1026
             * result = 14 02 36 01 + data
             * offset = 4;remaindata = 2046;
             * step 2:offset = 1026  ---error;should be 1024
             */
            //加上Server ID & Block Sqe
            int payloadSize = Math.Min(RemainingData + 2, MaxNumOfBlockLength);

            byte blockSeq = (byte)(SendedIndex % (0xFF + 1));
            if (payloadSize > 0xFFF)
            {
                result.Add(0x10);
                result.Add(0x00);
                result.AddRange(ByteExtension.GetBytes(payloadSize, true));
                FirstFrameMaxByteCount = MsgMaxLength - 2 - 4 - 2;
            }
            else
            {
                result.Add((byte)(0x10 | (payloadSize >> 8)));
                result.Add((byte)(payloadSize & 0xff));
                FirstFrameMaxByteCount = MsgMaxLength - 2 - 2;
            }
            result.Add((byte)CurrentUDSFunction);
            result.Add(blockSeq);

            //add data
            result.AddRange(SendDatas.Skip(Offset).Take(FirstFrameMaxByteCount));

            Offset += FirstFrameMaxByteCount;
            RemainingData -= FirstFrameMaxByteCount;

            return FillFrame(result);
        }

        private int _payloadSize = 0;
        /// <summary>
        /// 组连续帧
        /// </summary>
        private void BuildConsectiveFrame()
        {
            if (RemainingData <= 0)
                return;

            List<byte> result = new List<byte>();

            if (BS == 0)//剩余的全发
            {
                PayloadSize = Math.Min(RemainingData, ReadLen - FirstFrameMaxByteCount);//减去首帧发送的数据长度

                if (PayloadSize > 0)
                    ProgressInt = (int)((double)Offset / SendDatas.Length * 100);
                else
                    ProgressInt = 99;
                DebugLogger($"{CurrentStep} seq {SendedIndex}; sended bytecount:{Offset}. total:{SendDatas.Length}");
                byte[] blockBytes = SendDatas.Skip(Offset).Take(PayloadSize).ToArray();

                int frameCount = (int)Math.Ceiling((double)PayloadSize / ConsectiveFrameMaxLength);

                for (int j = 0; j < frameCount; j++)
                {
                    byte seq = (byte)((j + 1) % 0x10);
                    //data.Add((byte)(0x20 + seq));
                    result.Add((byte)(0x20 + seq));

                    int startidx = j * ConsectiveFrameMaxLength;
                    int leftdata = (blockBytes.Length - startidx);
                    int takebytes = leftdata > ConsectiveFrameMaxLength ? ConsectiveFrameMaxLength : leftdata;
                    for (int m = 0; m < takebytes; m++)
                    {
                        result.Add(blockBytes[startidx + m]);
                    }
                }
                //DebugLogger($"{CurrentStep} seq {SendedIndex}; send bytecount:{Offset} + {_payloadSize}. total:{SendDatas.Length}");
                Offset += PayloadSize;
                RemainingData -= PayloadSize;
                SendedIndex++;
            }
            else
            {
                /** Test
                 * send 2050 bytes data
                 * Step 1.0:
                 * blockLeftBytes = 1026 - 4+(56*0) % 1026 = 1022; --- error :1026,should 1024
                 * payloadSize = min(1022,2046) = 1022;
                 * payloadSize = min(1022,56) = 56;
                 * blockBytes = SendDatas.Skip(4).Take(56)
                 * ...
                 * bScount = 1;
                 * offset = 4 + 56 = 60;
                 * remain = 2050 - 4 - 56 * 1 = 1990;
                 * 
                 * Step 1.1
                 * blockLeftBytes = 1026 - (4+56*1) % 1026 = 966;
                 * payloadSize = min(min(966,1990), 56) = 56;
                 * blockBytes = SendDatas.Skip(60).take(56);
                 * ...
                 * bSCount = 2;
                 * offset = 4 + 56 * 2 = 116;
                 * remain = 2050 - 4 - 56 * 2 = 1934;
                 * ...
                 * Step 1.17
                 * blockLeftBytes = 1026 - (4+ 56 * 17) % 1026 = 70;
                 * payloadSize = min(min(70,(2050 - 4 - 56 * 17) , 56) = 56;
                 * blockBytes = SendDatas.Skip(4 + 56 * 17).take(56);
                 * ...
                 * bSCount = 18
                 * offset = 4 + 56 * 17;
                 * remain = 2050 - 4 - 56 * 17;
                 * step 1.18
                 * blockLeftBytes = 1026 -(4+56*18) %1026 = 14
                 * payloadSize = min(min(14,(2050 - 4-56*18),56) = 14
                 * blockBytes = SendDatas.Skip(4 + 56 * 18).take(14);
                 * ...
                 * bSCount = 19
                 * offset = 4+56*18+14 = 1026;
                 */

                //发送BS条数据
                int blockLeftBytes = ReadLen - Offset % ReadLen;
                PayloadSize = Math.Min(blockLeftBytes, RemainingData);
                PayloadSize = Math.Min(PayloadSize, BS * ConsectiveFrameMaxLength);

                if (PayloadSize > 0)
                    ProgressInt = (int)((double)Offset / SendDatas.Length * 100);
                else
                    ProgressInt = 99;
                DebugLogger($"{CurrentStep} seq {SendedIndex}; sended bytecount:{Offset}. total:{SendDatas.Length}");
                byte[] blockBytes = SendDatas.Skip(Offset).Take(PayloadSize).ToArray();

                int sendCount = (int)Math.Ceiling((double)(PayloadSize) / ConsectiveFrameMaxLength);
                //int remainBytes = (datas.Length - MAXDataLength) % 7;
                for (int j = 0; j < sendCount; j++)
                {
                    byte seq = (byte)((j + 1 + _bSCount * BS) % 0x10);
                    //data.Add((byte)(0x20 + seq));
                    result.Add((byte)(0x20 + seq));

                    int startidx = j * ConsectiveFrameMaxLength;
                    int leftdata = (blockBytes.Length - startidx);
                    int takebytes = leftdata > ConsectiveFrameMaxLength ? ConsectiveFrameMaxLength : leftdata;
                    for (int m = 0; m < takebytes; m++)
                    {
                        result.Add(blockBytes[startidx + m]);
                    }
                }

                _bSCount++;
                //DebugLogger($"{CurrentStep} seq {SendedIndex}; send bytecount:{Offset} + {_payloadSize}. total:{SendDatas.Length}");
                Offset += PayloadSize;
                RemainingData -= PayloadSize;
                if (BS * ConsectiveFrameMaxLength * _bSCount > ReadLen || BS * ConsectiveFrameMaxLength * _bSCount > SendDatas.Length)
                    SendedIndex++;
            }

            var senddata = FillFrame(result);

            SendFrames(senddata);

            //if (senddata.Length > MsgMaxLength)//
            //{
            //    //SendFrame = new CANSendFrame((int)PhyID_Req, senddata.Take(MsgMaxLength).ToArray(), IsCanFD);
            //    // Console.WriteLine($"{DateTime.Now:HH:mm:ss fff} 组装 Frame.");
            //    CurrentStatue = ServerStatus.Sending;
            //    needSendFrame = new Queue<CANSendFrame>();
            //    for (int i = 0; i < senddata.Length / MsgMaxLength; i++)
            //    {
            //        var send = new CANSendFrame((int)PhyID_Req, senddata.Skip(MsgMaxLength * i).Take(MsgMaxLength).ToArray(), IsCanFD);
            //        needSendFrame.Enqueue(send);
            //    }
            //}
            //else
            //{
            //    SendFrame = new CANSendFrame((int)PhyID_Req, senddata, IsCanFD);
            //}
        }

        protected override void ReSend(int retryCount)
        {
            //第一次重试时
            //回滚至上一个序列
            if (retryCount == 1)
                if (previousDatas != null && previousDatas.Length > 0)
                {
                    if ((previousDatas[0] & 0xF0) == 0x10)//int frameType = receive[0] & 0xf0;
                    {
                        Offset -= FirstFrameMaxByteCount;
                        RemainingData += FirstFrameMaxByteCount;
                    }
                    else
                    {
                        ///TODO:考虑BS!=0的情况
                        Offset = Offset - PayloadSize - FirstFrameMaxByteCount;
                        RemainingData = RemainingData + PayloadSize + FirstFrameMaxByteCount;
                        SendedIndex--;
                    }
                    DebugLogger($"{CurrentStep} Retry:offset rollback to {Offset}");
                    previousDatas = BuildFirstFrameByOffset();
                    //SendFrames(previousDatas);
                }

            base.ReSend(retryCount);
        }

        protected override void ParseFlowControlFrame(byte[] receive)
        {
            int state = receive[0] & 0x0f;
            if (state == 0)
            {
                //发送流控帧
                //Result = UDSResponse.FlowControl;
                STmin = receive[2];
                BS = receive[1];
                CurrentStatue = ServerStatus.Sending;
                //BuildContinueFrames();
                BuildConsectiveFrame();
                SendImmediately = true;
                SendContinueFrames();
                //DebugLog?.Invoke($"{CurrentStep} flowcontrol done");
                CurrentStatue = ServerStatus.WaitReceive;
            }
            else if (state == 1)
            {
                //break;
            }
            else if (state == 2)
            {
                CurrentStatue = ServerStatus.Done;
                throw new UDSException("流控帧溢出");
            }
            else
            {
                CurrentStatue = ServerStatus.Done;
                throw new UDSException($"未知：{receive[0]:X2}");
            }
        }

    }

    public class UploadDataTransferServer : DataTransferServer
    {
        public int DataLength { get; set; }
        public List<byte> UploadedData { get => _buffer; }

        public UploadDataTransferServer(uint slaver, uint master, IDevice device, ILogService logService,string custom = "") : base(slaver, master, device,logService, custom)
        {
            //UploadedData = new List<byte>();
        }

        public override byte[] BuildFrame()
        {
            if (UploadedDataLength >= DataLength)
                return null;

            int index = UploadedDataLength / ReadLen + 1;

            return FillFrame(new List<byte> { 0x02, 0x36, (byte)index });
        }

        protected override void ParseFirstFrmame(byte[] receive)
        {
            recieveByteLength = ((receive[0] & 0x0f) * 0x100) + (receive[1] & 0xff);//数据长度
            //_receiveDataCount = (length_ff - 6) / 7 + ((((length_ff - 6) % 7) > 0) ? 1 : 0);
            //_receiveDataCount = length_ff - 3;
            //_buffer.Clear();
            _buffer.AddRange(receive.ToList().Skip(4));

            SendFrame = new CanFrame(PhyID_Req, UDSHelper.Frame0x30, extendedFrame: IDExtended, IsCanFD, fillData: FillData);
            previousDatas = UDSHelper.Frame0x30;
            CurrentStatue = ServerStatus.WaitReceive;
        }

        /// <summary>
        /// 解析首帧时有多余的2个byte
        /// </summary>
        public int UploadedDataLength
        {
            get
            {
                if (UploadedData.Count == 0) return 0;
                return UploadedData.Count;
            }
        }
        protected override void ParseContinueFrame(byte[] receive)
        {
            //DebugLogger($"ParseContinueFrame :{string.Join(" ", receive.Select(x => x.ToString("X2")))}");
            int unTakedDataLength = DataLength - UploadedDataLength;
            int seqUnTakedDataLength = (recieveByteLength - 2) - UploadedDataLength % ReadLen;//减去76 seqIndex两个byte
            seqUnTakedDataLength = Math.Min(unTakedDataLength, seqUnTakedDataLength);
            int frameLength = receive.Length - 1;
            int takeLength = Math.Min(seqUnTakedDataLength, frameLength);
            ProgressInt = (int)((double)UploadedData.Count / DataLength * 100);
            UploadedData.AddRange(receive.Skip(1).Take(takeLength));
            //DebugLogger($"uploaded:{UploadedData.Count}");
            if (UploadedDataLength == DataLength)//接收完毕
            {
                DebugLogger($"upload Done.Target Length :{DataLength}, Actual:{UploadedData.Count}.");
                ParseData(receive);
            }
            else if (UploadedDataLength % ReadLen == 0)//已接受1次读取的数据，需要再发送
            {
                DebugLogger($"upload next ReadLen:Uploaded:{UploadedDataLength}");
                //DebugLogger("Upload Next");
                base.ParseData(receive);
                //BuildFrame();
                CurrentStatue = ServerStatus.WaitReceive;
            }
            else
            {
                DebugLogger($"uploaded:{UploadedData.Count}");
                CurrentStatue = ServerStatus.Normal;
            }

            //base.ParseContinueFrame(receive);
        }

        public override void ParseData(byte[] data)
        {
            if (UploadedData.Count >= DataLength)
            {
                CurrentStatue = ServerStatus.Done;
                Result = UDSResponse.Positive;
                //ResultMsg = $"{CurrentStep}：{DIDInfo.Data}";
                ProgressInt = 100;
                return;
            }
            else
            {
                //单帧
                UploadedData.AddRange(data.Skip(3).Take(DataLength - UploadedData.Count));
                //base.ParseData(data);
                if (UploadedData.Count < DataLength)
                {
                    base.ParseData(data);
                }
                else
                {
                    CurrentStatue = ServerStatus.Done;
                }
            }
        }

        protected override void ReSend(int retryCount)
        {
            DebugLogger($"{CurrentStep} Not Support ReSend!");
            //base.ReSend(retryCount);
        }


        public override void Reset()
        {
            //base.Reset();
            UploadedData.Clear();
        }
    }

    public class Erad5ReadMemoryServer : ComplexServers
    {
        public Erad5ReadMemoryServer(int normalTimeout, int pendingTimeout, IDevice device, ILogService logService,string name = null) : base(normalTimeout, pendingTimeout, device, logService)
        {
            if (string.IsNullOrEmpty(name))
                this.Name = "eRad5 Memory Read";
            else
                Name = name;
            UploadData = new Dictionary<uint, List<byte>>();
        }
        public Dictionary<uint, List<byte>> UploadData { get; }
        public override void LoadServers()
        {
            Servers = new List<MultipServer>();
            MultipServer flow01 = new MultipServer(NormalTimeout, PendingTimeout, Device, Log);
            flow01.Name = "Read Memory";
            Add(flow01);

            var diagnosticSession03 = new DiagnosticSessionControlServer(PhyID_Res, FunctionID, Device, Log)
            {
                SubFuncCode = 0x03
            };
            flow01.Add(diagnosticSession03);

            SercureAccessServer security0x2701 = new SercureAccessServer(PhyID_Res, PhyID_Req, Device, Log);
            security0x2701.SubFuncCode = 0x01;
            flow01.Add(security0x2701);

            for (int i = 0; i < BinTmpFile.Segments.Count; i++)
            {
                UniversalServer server0x34_app = new UniversalServer(PhyID_Res, PhyID_Req,
                   $"RequestUpload seg {i}, Addr : 0x{string.Join("", BinTmpFile.Segments[i].DataStartAddress.Select(x => x.ToString("X2")))}",
                   UDSServerCode.RequestUpload, Device, Log);
                //server0x28.SubFunc = 0x03;
                var appinfo = new List<byte> { 0x00, 0x44 };
                appinfo.AddRange(BinTmpFile.Segments[i].DataStartAddress);
                appinfo.AddRange(BinTmpFile.Segments[i].Length);
                server0x34_app.SendDatas = appinfo.ToArray();// add start position and app data length
                server0x34_app.ParseForParameter = new Func<byte[], object>((x) => { return new Result0x34(x); });
                flow01.Add(server0x34_app);

                UploadDataTransferServer transferServer_app = new UploadDataTransferServer(PhyID_Res, PhyID_Req, Device, Log, $"DataTransfer seg{i}");
                var lengthArr = BinTmpFile.Segments[i].Length.Reverse().ToArray();
                //lengthArr.;
                transferServer_app.DataLength = BitConverter.ToInt32(lengthArr, 0);
                var appinfo2 = new List<byte>();
                //appinfo2.AddRange(BinTmpFile.App_DataBuffer);
                //appinfo2.AddRange(BinTmpFile.Segments[i].Data);
                transferServer_app.SendDatas = appinfo2.ToArray();
                flow01.Add(transferServer_app);
                var addr = BitConverter.ToUInt32(BinTmpFile.Segments[i].DataStartAddress.Reverse().ToArray(), 0);
                transferServer_app.RunCompleted = (x) =>
                {
                    if (x.UDSResponse == UDSResponse.Positive)
                        UploadData.Add(addr, transferServer_app.UploadedData);
                };

                UniversalServer server0x37_01 = new UniversalServer(PhyID_Res, PhyID_Req, $"RequestTransferExit seg{i}", UDSServerCode.RequestTransferExit, Device, Log);
                flow01.Add(server0x37_01);
            }
        }

        public override void ResetResult()
        {
            UploadData.Clear();
            base.ResetResult();
        }
    }

    public class Erad5WriteMemoryServer : ComplexServers
    {
        public Erad5WriteMemoryServer(int normalTimeout, int pendingTimeout, IDevice device, ILogService logService) : base(normalTimeout, pendingTimeout, device, logService)
        {
            Name = "eRad5 Write";
        }

        public override void LoadServers()
        {
            Servers = new List<MultipServer>();
            MultipServer flow01 = new MultipServer(NormalTimeout, PendingTimeout, Device, Log);
            flow01.Name = "Write Memory";
            Add(flow01);

            var diagnosticSession03 = new DiagnosticSessionControlServer(PhyID_Res, FunctionID, Device, Log)
            {
                SubFuncCode = 0x03
            };
            flow01.Add(diagnosticSession03);

            SercureAccessServer security0x2701 = new SercureAccessServer(PhyID_Res, PhyID_Req, Device, Log);
            security0x2701.SubFuncCode = 0x01;
            flow01.Add(security0x2701);

            for (int i = 0; i < BinTmpFile.Segments.Count; i++)
            {
                //擦除app 数据
                RoutineControlServer server0x3101_Erase_Flash = new RoutineControlServer(PhyID_Res, PhyID_Req, Device, Log)
                {
                    Name = "FF 00 44 RountineControl",
                    SubFuncCode = 0x01
                };
                var flashinfo_EraseFlash = new List<byte> { 0xff, 0x00, 0x44 };
                //flashinfo_EraseFlash.AddRange(BinTmpFile.App_MEM_START_ADDR);
                //flashinfo_EraseFlash.AddRange(BinTmpFile.App_Data_Length_bytes);
                flashinfo_EraseFlash.AddRange(BinTmpFile.Segments[i].DataStartAddress);
                flashinfo_EraseFlash.AddRange(BinTmpFile.Segments[i].Length);
                server0x3101_Erase_Flash.SendDatas = flashinfo_EraseFlash.ToArray();
                flow01.Add(server0x3101_Erase_Flash);

                UniversalServer server0x34_app = new UniversalServer(PhyID_Res, PhyID_Req,
                    $"RequestDownload seg {i}:{string.Join("", BinTmpFile.Segments[i].DataStartAddress.Select(x => x.ToString("X2")))}",
                    UDSServerCode.RequestDownload, Device, Log);
                //server0x28.SubFunc = 0x03;
                var appinfo = new List<byte> { 0x00, 0x44 };
                //appinfo.AddRange(BinTmpFile.App_MEM_START_ADDR);
                //appinfo.AddRange(BinTmpFile.App_Data_Length_bytes);
                appinfo.AddRange(BinTmpFile.Segments[i].DataStartAddress);
                appinfo.AddRange(BinTmpFile.Segments[i].Length);
                server0x34_app.SendDatas = appinfo.ToArray();// add start position and app data length
                server0x34_app.ParseForParameter = new Func<byte[], object>((x) => { return new Result0x34(x); });
                flow01.Add(server0x34_app);

                var transferServer_app = new DataTransferServer(PhyID_Res, PhyID_Req, Device, Log, $"DataTransfer seg{i}");

                var appinfo2 = new List<byte>();
                //appinfo2.AddRange(BinTmpFile.App_DataBuffer);
                appinfo2.AddRange(BinTmpFile.Segments[i].Data);
                transferServer_app.SendDatas = appinfo2.ToArray();
                flow01.Add(transferServer_app);

                UniversalServer server0x37_01 = new UniversalServer(PhyID_Res, PhyID_Req, $"RequestTransferExit seg{i}", UDSServerCode.RequestTransferExit, Device, Log);
                flow01.Add(server0x37_01);

            }
        }
    }
}
