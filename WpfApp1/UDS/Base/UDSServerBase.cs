using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ERad5TestGUI.Devices;
using ERad5TestGUI.Helpers;
using ERad5TestGUI.Services;

namespace ERad5TestGUI.UDS
{

    public abstract class UDSServerBase : UDSServerAbstract
    {
        public Func<byte[], object> ParseForParameter;
        protected List<byte> _buffer = new List<byte>();
        protected Queue<IFrame> needSendFrame;
        protected AutoResetEvent ReceiveEvent;
        protected int recieveByteLength;
        private const int CANFDMSGMAXLENGTH = 64;
        private const int CANMSGMAXLENGTH = 8;
        private IFrame _receiveFrame;
        private IFrame _sendFrame;
        private ServerStatus _status;
        private int progressInt;
        private UDSResponse result;
        private string resultMsg;
        private string sendAndReceiveStr = String.Empty;
        private ServerResult serverResult;

        public UDSServerBase(uint slaver, uint master, IDevice device, ILogService logService, bool isCanFD = false)
            : base(device, logService)
        {
            PhyID_Res = slaver;
            PhyID_Req = master;
            FunctionID = 0x7DF;
            ReceiveEvent = new AutoResetEvent(false);
            //receiveData = new Queue<CANReceiveFrame>();
            ErrCode = new Dictionary<byte, string>()
            {
                { 0x10, "General Reject" },
                { 0x11, "service not support" },
                { 0x12, "sub func not support" },
                { 0x13, "incorrect Msg Length or Invalid Format" },
                { 0x22, "conditions not correct" },
                { 0x24, "request sequence error" },
                { 0x31, "request of range" },
                { 0x33, "security access denied" },
                { 0x71, "transfer data suspended" },
                { 0x72, "General program fail" },
                { 0x73, "wrong block sqe counter" },
                { 0x78, "request correctly received-response pending" },
                { 0x7e, "sub func not support" },
                { 0x7f, "serv not support in active session" },
                { 0x92, "Voltage too high" },
                { 0x93, "Voltage too low" },
            };
            IsCanFD = isCanFD;
        }

        //public override event OutputLog DebugLog;

        ///// <summary>
        ///// 接收数据事件
        ///// </summary>
        //public override event RegisterRecieve RegisterRecieveEvent;

        //public override event SendData Send;

        //public override event SendMultipData SendMultip;

        /// <summary>
        /// 执行后传给下一流程的参数
        /// </summary>
        public object AfterRunParamter { get; protected set; }

        /// <summary>
        /// 连续帧最长发送数据长度
        /// <para>can 2.0: 7</para>
        /// <para>canFD  : 63</para>
        /// </summary>
        public int ConsectiveFrameMaxLength { get => !IsCanFD ? 7 : 63; }

        /// <summary>
        /// 当前服务是否结束
        /// </summary>
        public ServerStatus CurrentStatue
        {
            get => _status;
            set
            {
                if (_status == ServerStatus.Done && Result != UDSResponse.Init)//已经结束就不能更改状态
                    return;
                //相同状态不用改变
                if (!SetProperty(ref _status, value))
                {
                    return;
                }
                if (_status == ServerStatus.Done)
                    ReceiveSet();
                DebugLogger($"{CurrentStep} {value}");
            }
        }

        private void ReceiveSet()
        {
            try
            {
                ReceiveEvent.Set();
            }
            catch (Exception)
            {

            }
        }

        public string CurrentStep
        {
            get => $"0x{((byte)CurrentUDSFunction):X2} {SubFuncCode:X2} {ServerName} [0x{PhyID_Res:X}]";
        }

        /// <summary>
        /// 当前的超时时间
        /// </summary>
        public int CurrentTimeout { get; protected set; }

        /// <summary>
        /// 错误码含义
        /// </summary>
        public Dictionary<byte, string> ErrCode { get; private set; }

        /// <summary>
        /// 一帧报文最长的byte数
        /// </summary>
        public int MsgMaxLength { get => IsCanFD ? CANFDMSGMAXLENGTH : CANMSGMAXLENGTH; }

        public bool NeedReceive { get; set; } = true;

        public new int ProgressInt
        {
            get => progressInt;
            set
            {
                if (SetProperty(ref progressInt, value))
                {
                    ServerResult.Progress = value;

                    ServerResult copy = new ServerResult(Index, ProgressWeights)
                    {
                        UDSResponse = Result,
                        Message = this.ResultMsg,
                        Progress = progressInt
                    };

                    Progress?.Report(copy);
                }
            }
        }

        public IFrame ReceiveFrame
        {
            get => _receiveFrame;
            set
            {
                if (!SetProperty(ref _receiveFrame, value) || _receiveFrame == null)
                    return;
                SendAndReceiveStr += $"Receive:{_receiveFrame} \r";
                DebugLogger($"{CurrentStep} receive {_receiveFrame}");
                CurrentStatue = ServerStatus.Normal;
                //ReceiveData = true;
                try
                {
                    ReceiveEvent.Set();
                    CurrentTimeout = NormalTimeout;
                    DebugLogger($"{CurrentStep} Start Prase {_receiveFrame}");
                    ParseResponse(_receiveFrame.Data);
                }
                catch (UDSException e)
                {
                    Result = UDSResponse.ParseError;
                    ResultMsg = e.Message;
                    CurrentStatue = ServerStatus.Done;
                }
                catch (Exception e)
                {
                    Result = UDSResponse.Unknow;
                    ResultMsg = e.Message;
                    CurrentStatue = ServerStatus.Done;
                }
                finally
                {
                    DebugLogger($"{CurrentStep} receive 【{_receiveFrame}】 done.");
                }
            }
        }

        /// <summary>
        /// 服务运行的最终结果/返回的报文解析结果
        /// </summary>
        public new UDSResponse Result
        {
            get => result;
            set
            {
                //if(result == UDSResponse.Cancel)

                SetProperty(ref result, value);
                if (ServerResult == null)
                    ServerResult = new ServerResult(Index, ProgressWeights);
                ServerResult.UDSResponse = value;
            }
        }

        public new string ResultMsg
        {
            get => resultMsg;
            set
            {
                SetProperty(ref resultMsg, value);
                if (ServerResult == null)
                    ServerResult = new ServerResult(Index, ProgressWeights);
                ServerResult.Message = value;
            }
        }

        public string SendAndReceiveStr
        {
            get => sendAndReceiveStr;
            set
            {
                if (value.Length > 10000) return;
                SetProperty(ref sendAndReceiveStr, value);
            }
        }

        /// <summary>
        /// 发送的数据，不包括subfuc
        /// </summary>
        public byte[] SendDatas { get; set; }

        public IFrame SendFrame
        {
            get => _sendFrame; set
            {
                if (!SetProperty(ref _sendFrame, value) || value == null)
                    return;
                ReceiveEvent.Reset();
                if (Device == null)
                {
                    Result = UDSResponse.SendFail;
                    ResultMsg = $"未绑定发送事件";
                    this.CurrentStatue = ServerStatus.Done;
                }
                else
                {
                    var sendres = Device.SendFD(_sendFrame);

                    if (!sendres)
                    {
                        Result = UDSResponse.SendFail;
                        ResultMsg = $"{CurrentStep} CAN {CanChannel} 发送失败";
                        this.CurrentStatue = ServerStatus.Done;
                    }
                    SendAndReceiveStr += $"Send   :{_sendFrame}\r";
                    DebugLogger($"{CurrentStep} Send    {_sendFrame} {sendres}");
                }
            }
        }

        /// <summary>
        /// 发送连续帧时，每帧的间隔事件，以 ms 计
        /// </summary>
        public int SendInterval { get; set; } = 0;

        public string ServerName
        {
            get => Name;
            protected set => Name = value;
        }

        public ServerResult ServerResult
        {
            get => serverResult;
            protected set
            {
                SetProperty(ref serverResult, value);
            }
        }

        /// <summary>
        /// 单帧可发送的最长数据长度，大于这个数量则需要改为首帧+连续帧发送
        /// <para>can2.0: 8 - 1(length) - 1(ServerID) - 1(SubFuncID) = 5 / 6</para>
        /// <para>can FD: 64 - 1 or 2(length) - 1(ServerID) - 1(SubFuncID) = 61 or 60</para>
        /// <para>待确认</para>
        /// </summary>
        public int SingleFrameCanSendByteCount
        {
            get
            {
                if (IsCanFD) return 61;
                return 5;
            }
        }
        /// <summary>
        /// 子功能代码
        /// </summary>
        public byte? SubFuncCode { get; set; } = null;

        /// <summary>
        /// 最大连续帧数
        /// <para>发送BS数值的连续帧后，需要等待接收方的流控帧，0表示不需要等待</para>
        /// </summary>
        protected byte BS { get; set; } = 0;

        /// <summary>
        /// 连续帧之间的最小时间间隔，ms
        /// </summary>
        protected byte STmin { get; set; } = 0;
        public abstract UDSServerCode CurrentUDSFunction { get; protected set; }

        public Action<ServerResult> RunCompleted;
        public Action BeforeRunDo;

        /// <summary>
        /// 组帧
        /// </summary>
        /// <returns>8/64位byte数据</returns>
        public virtual byte[] BuildFrame()
        {
            List<byte> data = new List<byte>();
            int maxLength = SingleFrameCanSendByteCount + (SubFuncCode.HasValue ? -1 : 0);
            //发送的数据长度
            int sendDataCount = SendDatas == null ? 0 : SendDatas.Length;
            //加上服务ID/自服务ID 长度
            int sendByteCount = 1 + (SubFuncCode.HasValue ? 1 : 0) + sendDataCount;

            if (IsCanFD)
            {
                if (sendByteCount > 62)//首帧+连续帧
                {
                    if (sendByteCount > 4095)
                    {
                        maxLength -= 1;
                        throw new NotSupportedException();
                    }
                    else
                    {
                        //发首帧...
                        //数据长度
                        DebugLogger($"{CurrentStep} 组首帧，连续帧：DataLength {SendDatas.Length}");
                        int length = sendByteCount;
                        byte databyte0 = (byte)(0x10 | (length >> 8));
                        data.Add(databyte0);
                        byte databyte1 = (byte)(length & 0xff);
                        data.Add(databyte1);

                        data.Add((byte)CurrentUDSFunction);
                        if (SubFuncCode.HasValue)
                            data.Add((byte)SubFuncCode.Value);
                        //add data
                        data.AddRange(SendDatas.Take(maxLength));
                        int singleFrameBytes = ConsectiveFrameMaxLength;
                        int sendCount = (int)Math.Ceiling((double)(SendDatas.Length - maxLength) / (singleFrameBytes));
                        //int remainBytes = (datas.Length - MAXDataLength) % 7;
                        for (int i = 0; i < sendCount; i++)
                        {
                            byte seq = (byte)((i + 1) % 0x10);
                            data.Add((byte)(0x20 + seq));
                            if (SendDatas.Skip(maxLength + i * singleFrameBytes).Count() < singleFrameBytes)
                            {
                                data.AddRange(SendDatas.Skip(maxLength + (i * singleFrameBytes)));
                            }
                            else
                            {
                                data.AddRange(SendDatas.Skip(maxLength + i * singleFrameBytes).Take(singleFrameBytes));
                            }
                        }
                    }
                }
                else//发送单帧
                {
                    if (sendByteCount > 7) //length 在 byte1 中
                    {
                        data.Add((byte)CurrentUDSFunction);
                        if (SubFuncCode.HasValue)
                            data.Add(SubFuncCode.Value);
                        if (SendDatas != null)
                            data.AddRange(SendDatas);
                        int length = data.Count;
                        data.Insert(0, (byte)length);//Length
                        data.Insert(0, 0);//data.Add(0x00);//Length
                    }
                    else//byte length 在 byte0 中
                    {
                        data.Add((byte)CurrentUDSFunction);
                        if (SubFuncCode.HasValue)
                            data.Add(SubFuncCode.Value);
                        if (SendDatas != null)
                            data.AddRange(SendDatas);
                        int length = data.Count;
                        data.Insert(0, (byte)length);//data.Add(0x00);//Length
                    }
                }
            }
            else //can 2.0
            {
                if (SendDatas != null && SendDatas.Length > maxLength)//数据超过单帧发送长度，需发送首帧+连续帧
                {
                    //发首帧...
                    //数据长度
                    DebugLogger($"{CurrentStep} 组首帧，连续帧：DataLength {SendDatas.Length}");
                    int length = SendDatas.Length + (MsgMaxLength - maxLength - 2);
                    byte databyte0 = (byte)(0x10 | (length >> MsgMaxLength));
                    data.Add(databyte0);
                    byte databyte1 = (byte)(length & 0xff);
                    data.Add(databyte1);

                    data.Add((byte)CurrentUDSFunction);
                    if (SubFuncCode.HasValue)
                        data.Add(SubFuncCode.Value);
                    //add data
                    data.AddRange(SendDatas.Take(maxLength));
                    int singleFrameBytes = ConsectiveFrameMaxLength;
                    int sendCount = (int)Math.Ceiling((double)(SendDatas.Length - maxLength) / (singleFrameBytes));
                    //int remainBytes = (datas.Length - MAXDataLength) % 7;
                    for (int i = 0; i < sendCount; i++)
                    {
                        byte seq = (byte)((i + 1) % 0x10);
                        data.Add((byte)(0x20 + seq));
                        if (SendDatas.Skip(maxLength + i * singleFrameBytes).Count() < singleFrameBytes)
                        {
                            data.AddRange(SendDatas.Skip(maxLength + (i * singleFrameBytes)));
                        }
                        else
                        {
                            data.AddRange(SendDatas.Skip(maxLength + i * singleFrameBytes).Take(singleFrameBytes));
                        }
                    }
                }
                else
                {
                    data.Add((byte)CurrentUDSFunction);
                    if (SubFuncCode.HasValue)
                        data.Add(SubFuncCode.Value);
                    if (SendDatas != null)
                        data.AddRange(SendDatas);
                    int length = data.Count;
                    data.Insert(0, (byte)length);//data.Add(0x00);//Length
                }
            }
            return FillFrame(data);
        }

        public void DebugLogger(string log)
        {
            Log?.LogUds(log);
        }

        public void ModifyErrCode(byte errCode, string errMsg)
        {
            if (ErrCode.ContainsKey(errCode))
            {
                ErrCode[errCode] = errMsg;
            }
            else
            {
                ErrCode.Add(errCode, errMsg);
            }
        }

        /// <summary>
        /// 解析数据后的操作
        /// </summary>
        /// <param name="data"></param>
        public virtual void ParseData(byte[] data)
        {
            AfterRunParameter = ParseForParameter?.Invoke(data);
            CurrentStatue = ServerStatus.Done;
        }

        /// <summary>
        /// 解析接收数据
        /// </summary>
        /// <param name="receive"></param>
        /// <returns></returns>
        /// <exception cref="UDSException"></exception>
        public virtual bool ParseResponse(byte[] receive)
        {
            //剔除 0xAA
            //receive = receive.ToList().re
            //Result = UDSResponse.Init;
            bool result = false;
            int frameType = receive[0] & 0xf0;
            switch ((FrameType)frameType)
            {
                case FrameType.SingleFrame://单帧
                    result = ParseSingleFrame(receive);
                    break;
                case FrameType.FirstFrame:
                    ParseFirstFrmame(receive);
                    break;
                case FrameType.ContinueFrame:
                    ParseContinueFrame(receive);
                    break;
                case FrameType.FlowControlFrame:
                    ParseFlowControlFrame(receive);
                    break;
            }

            //CurrentTimeout = NormalTimeout;
            //CurrentStatue = ServerStatus.Done;
            return result;
        }

        /// <summary>
        /// 重置属性/字段，为再次调用做准备
        /// </summary>
        public virtual void Reset()
        {
            //ReceiveEvent = new AutoResetEvent(false);
        }

        public override void ResetResult()
        {
            SendFrame = null;
            ReceiveFrame = null;
            Result = UDSResponse.Init;
            ResultMsg = "";
            ProgressInt = 0;
            SendAndReceiveStr = string.Empty;
            ReceiveEvent = new AutoResetEvent(false);
        }

        public virtual ServerResult Run(object param = null)
        {
            try
            {
                Reset();
                StartOrStopRec(CanChannel, true);
                ServerResult = new ServerResult(Index, ProgressWeights);

                Result = UDSResponse.Init;
                //ResultMsg = $"{CurrentStep} {GlobalVar.START}.";
                ProgressInt = 10;

                CurrentTimeout = NormalTimeout;

                if (!NeedReceive)
                {
                    Result = UDSResponse.Positive;
                    //ResultMsg = "No receive.";
                    CurrentStatue = ServerStatus.Done;
                    ProgressInt = 50;
                }
                else
                {
                    CurrentStatue = ServerStatus.WaitReceive;
                }

                var data = BuildFrame();
                SendFrames(data);

                if (Result == UDSResponse.SendFail)
                {
                    ProgressInt = 100;
                    return ServerResult;
                }

                //ResultMsg = $"{CurrentStep} {GlobalVar.RUNNING}...";
                /** 
                 * 
                 */
                ProgressInt = 80;
                int waitCount = 0;
                int timeoutCount = 0;
                DateTime preDate = DateTime.Now;
                do
                {
                    if (this.cancelSource != null && this.cancelSource.IsCancellationRequested)
                    {
                        this.cancelSource.Token.ThrowIfCancellationRequested();
                    }

                    if (CurrentStatue == ServerStatus.WaitReceive)
                    {
                        waitCount++;
                        DebugLogger($"{CurrentStep} waitcount:{waitCount} start wait {CurrentTimeout} ms");
                        if (ReceiveEvent.WaitOne(CurrentTimeout))
                        {
                            DebugLogger($"{CurrentStep} waitcount:{waitCount} reset");
                            ReceiveEvent.Reset();
                            SendImmediately = false;
                            timeoutCount = 0;
                        }
                        else
                        {
                            if (timeoutCount >= 3)
                            {
                                DebugLogger($"{CurrentStep} waitcount:{waitCount} timeout,last receive:{ReceiveFrame}");
                                Result = UDSResponse.Timeout;
                                ResultMsg = $"{CurrentStep} waitcount:{waitCount} Time Out";
                                CurrentStatue = ServerStatus.Done;
                                break;
                            }
                            else
                            {
                                timeoutCount++;
                                //再等待1s，确保不是接收堵塞
                                WaitCache();
                                if (CurrentStatue == ServerStatus.WaitReceive)
                                    ReSend(timeoutCount);
                            }
                        }
                    }
                    if ((DateTime.Now - preDate).TotalSeconds > 2)
                    {
                        var _sendFrame = new CanFrame(FunctionID, UDSHelper.Frame0x3e80, isCanFD: IsCanFD, fillData: FillData, extendedFrame: IDExtended);
                        var sendres = Send(_sendFrame);

                        if (!sendres)
                        {
                            Result = UDSResponse.SendFail;
                            ResultMsg = $"{CurrentStep} CAN {CanChannel} Send Fail";
                            this.CurrentStatue = ServerStatus.Done;
                        }
                        preDate = DateTime.Now;
                    }
                    Thread.Sleep(1);

                } while (CurrentStatue != ServerStatus.Done);

            }
            catch(OperationCanceledException)
            {
                Result = UDSResponse.Cancel;
            }
            catch (Exception e)
            {
                Result = UDSResponse.Unknow;
                ResultMsg = e.Message + e.StackTrace;
            }
            finally
            {
                //if (Result == UDSResponse.Timeout)
                //    Thread.Sleep(1000);
                ReceiveEvent.Close();
                StartOrStopRec(CanChannel, false);
                
                this.SendAndReceiveStr = SendAndReceiveStr?.Trim();
            }

            if (ServerResult.UDSResponse == UDSResponse.Positive)
                ResultMsg = $"{CurrentStep} {END}.";
            ProgressInt = 100;

            return ServerResult;
        }
        private CancellationTokenSource cancelSource;
        /// <summary>
        /// 异步执行，并报告进度
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public override Task<ServerResult> RunAsync(CancellationTokenSource cancelSource = null, object param = null)
        {
            this.cancelSource = cancelSource;

            return Task.Run(() =>
            {
                BeforeRunDo?.Invoke();
                var res = Run(param);
                RunCompleted?.Invoke(res);
                return res;
            });

        }

        public ServerResult RunTimeoutRetry(int retryCount = 2)
        {
            int count = 0;
            Run();
            if (ServerResult.UDSResponse == UDSResponse.Timeout)
            {
                ServerName += " [timout]Retry " + "0";
                do
                {
                    ServerName = ServerName.Remove(ServerName.Length - 1, 1) + count.ToString();
                    count++;
                    Run();
                    if (ServerResult.UDSResponse != UDSResponse.Timeout)
                    {
                        break;
                    }
                } while (count < retryCount);
            }

            return ServerResult;
        }

        /// <summary>
        /// 由CANFrame填充 2024/12/19
        /// <para>补全8/64位长度的CAN报文，发送不满8位的数据，UDS不回应</para>
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected byte[] FillFrame(List<byte> data)
        {
            //int c = data.Count;

            //int datalength = CANSendFrame.GetDLCDataLength(c);

            //int formatLength = datalength - c;

            //for (int i = 0; i < formatLength; i++)
            //{
            //    data.Add(0xaa);
            //}

            return data.ToArray();
        }

        protected virtual void OnDataCommonRecieveEvent(IEnumerable<IFrame> can_msg)
        {
            try
            {
                foreach (var can in can_msg)
                {
                    if (GetId(can.MessageID) == PhyID_Res)
                    {
                        ReceiveFrame = can;
                    }
                }
            }
            catch (Exception err)
            {
                DebugLogger($"{CurrentStep} receive error: {err.Message}");
            }
        }

        private void OnMsgReceivedEvent(uint id, byte[] data, int dlc)
        {
            if (GetId(id) == PhyID_Res)
            {
                ReceiveFrame = new CanFrame(id, data, dlc: dlc);
            }
        }

        /// <summary>
        /// 解析连续帧
        /// </summary>
        /// <param name="receive"></param>
        protected virtual void ParseContinueFrame(byte[] receive)
        {
            int leftcount = recieveByteLength - _buffer.Count;

            if (leftcount <= ConsectiveFrameMaxLength)
            {
                _buffer.AddRange(receive.ToList().Skip(1).Take(leftcount));
            }
            else
            {
                _buffer.AddRange(receive.ToList().Skip(1));
            }

            if (_buffer.Count == recieveByteLength)
            {
                ParseData(_buffer.ToArray());
                CurrentStatue = ServerStatus.Done;
            }
            else
            {
                CurrentStatue = ServerStatus.WaitReceive;
            }
        }

        /// <summary>
        /// 解析首帧
        /// </summary>
        /// <param name="receive"></param>
        protected virtual void ParseFirstFrmame(byte[] receive)
        {
            recieveByteLength = ((receive[0] & 0x0f) * 0x100) + (receive[1] & 0xff);//数据长度
            //_receiveDataCount = (length_ff - 6) / 7 + ((((length_ff - 6) % 7) > 0) ? 1 : 0);
            //_receiveDataCount = length_ff - 3;
            _buffer.Clear();
            _buffer.AddRange(receive.ToList().Skip(2));

            SendFrame = new CanFrame(PhyID_Req, UDSHelper.Frame0x30, extendedFrame: IDExtended, IsCanFD, fillData: FillData);
            previousDatas = UDSHelper.Frame0x30;
            CurrentStatue = ServerStatus.WaitReceive;
        }

        /// <summary>
        /// 解析流控帧
        /// </summary>
        /// <param name="receive"></param>
        /// <exception cref="UDSException"></exception>
        protected virtual void ParseFlowControlFrame(byte[] receive)
        {
            int state = receive[0] & 0x0f;
            if (state == 0)
            {
                //发送流控帧
                //Result = UDSResponse.FlowControl;
                STmin = receive[2];
                BS = receive[1];
                CurrentStatue = ServerStatus.Sending;
                SendContinueFrames();
                DebugLogger($"{CurrentStep} flowcontrol done");
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

        /// <summary>
        /// 解析单帧的正响应
        /// </summary>
        /// <param name="receivedata"></param>
        protected virtual void ParsePositiveSingleFrame(byte[] receivedata)
        {
            DebugLogger($"{CurrentStep} Parse positive Single response ");
            Result = UDSResponse.Positive;
            ResultMsg = "";

            ParseData(receivedata);
        }

        protected virtual bool ParseSingleFrame(byte[] receive)
        {
            bool result = false;
            if (receive.Length > 8)
            {
                recieveByteLength = receive[1];

                if (recieveByteLength > 0)
                {
                    if (receive[2] - 0x40 == (int)CurrentUDSFunction)
                    {
                        Result = UDSResponse.Positive;
                        CurrentStatue = ServerStatus.Normal;
                        ParsePositiveSingleFrame(receive);
                        result = true;
                    }
                    else
                    {
                        if (receive[2] == 0x7f)
                        {
                            Result = UDSResponse.Negative;
                            ResultMsg = $"{CurrentStep}:{receive[1]:X2} {receive[2]:X2} {receive[3]:X2};";
                            if (ErrCode != null && ErrCode.ContainsKey(receive[3]))
                            {
                                ResultMsg += ErrCode[receive[3]];
                            }
                            CurrentStatue = ServerStatus.Done;
                        }
                        else
                        {
                            CurrentStatue = ServerStatus.WaitReceive;
                        }
                    }
                }
            }
            else
            {
                recieveByteLength = receive[0] & 0x0f;//数据长度

                if (recieveByteLength > 0)
                {
                    if (receive[1] - 0x40 == (int)CurrentUDSFunction)//正响应
                    {
                        Result = UDSResponse.Positive;
                        CurrentStatue = ServerStatus.Normal;
                        ParsePositiveSingleFrame(receive);
                        result = true;
                    }
                    else//负响应 or 
                    {
                        if (receive[3] == 0x78)//pending
                        {
                            //ProgressInt = -1;
                            CurrentTimeout = PendingTimeout;
                            CurrentStatue = ServerStatus.WaitReceive;
                            return true;
                        }
                        else if (receive[1] == 0x7f)
                        {
                            Result = UDSResponse.Negative;
                            ResultMsg = $"{CurrentStep}:{receive[1]:X2} {receive[2]:X2} {receive[3]:X2};";
                            if (ErrCode != null && ErrCode.ContainsKey(receive[3]))
                            {
                                ResultMsg += ErrCode[receive[3]];
                            }
                            CurrentStatue = ServerStatus.Done;
                        }
                        else
                        {
                            CurrentStatue = ServerStatus.WaitReceive;
                            //return true;
                        }
                    }
                }
                else
                {
                    CurrentStatue = ServerStatus.Done;
                    throw new UDSException("Receive Data Length Error ");
                }
            }

            return result;
        }

        /// <summary>
        /// 发送连续帧，Send from <see cref="needSendFrame"/>
        /// </summary>
        protected virtual void SendContinueFrames()
        {
            List<IFrame> frames = new List<IFrame>();
            while (needSendFrame != null && needSendFrame.Count > 0)
            {
                frames.Add(needSendFrame.Dequeue());
            }
            //只有STmin == 0 时 使用，sendmulti
            if (STmin == 0)
            {
                DebugLogger($"{CurrentStep} Send Multip {frames.Count}.");
                SendAndReceiveStr += $"Send Multip {frames.Count}. \r";
                if (!SendMultip(frames))//会堵塞，与上一帧发送间隔超过1s，多的5s
                {
                    this.Result = UDSResponse.SendFail;
                    CurrentStatue = ServerStatus.Done;
                }
            }
            else//发送每帧都要延时
            {
                foreach (var item in frames)
                {
                    SendFrame = item;
                    Thread.Sleep(STmin);
                }
            }

        }

        protected void SendFrames(byte[] data)
        {
            previousDatas = data;
            if (data.Length > MsgMaxLength)//
            {
                var frame = new CanFrame(PhyID_Req, data.Take(MsgMaxLength).ToArray(), extendedFrame: IDExtended, IsCanFD, dlc: CanFrame.GetDLCByDataLength(MsgMaxLength), fillData: FillData);
                DebugLogger($"{CurrentStep} 连续帧入栈：{data.Length / MsgMaxLength}");
                needSendFrame = new Queue<IFrame>();

                int count = data.Length % MsgMaxLength == 0 ? 0 : 1;
                count += data.Length / MsgMaxLength;
                for (int i = 1; i < count; i++)
                {
                    int cantakeLength = Math.Min(data.Skip(MsgMaxLength * i).Count(), MsgMaxLength);
                    var send = new CanFrame(PhyID_Req, data.Skip(MsgMaxLength * i).Take(cantakeLength).ToArray(), extendedFrame: IDExtended, IsCanFD, dlc: CanFrame.GetDLCByDataLength(cantakeLength), fillData: FillData);
                    needSendFrame.Enqueue(send);
                }
                //组帧完后再发送
                SendFrame = frame;
                
            }
            else
            {
                SendFrame = new CanFrame(PhyID_Req, data, extendedFrame: IDExtended, IsCanFD, dlc: CanFrame.GetDLCByDataLength(data.Length), fillData: FillData);
            }
        }
        /// <summary>
        /// 启动/关闭接收CAN报文
        /// </summary>
        /// <param name="index"></param>
        /// <param name="open"></param>
        protected void StartOrStopRec(int index, bool open)
        {
            if (Device != null)
                if (open)
                {
                    DebugLogger($"{CurrentStep} Start Receive.");
                    //Device.OnIFramesReceived += OnDataCommonRecieveEvent;
                    Device.OnMsgReceived += OnMsgReceivedEvent;
                }
                else
                {
                    //Device.OnIFramesReceived -= OnDataCommonRecieveEvent;
                    Device.OnMsgReceived -= OnMsgReceivedEvent;
                    DebugLogger($"{CurrentStep} Stop Receive.");
                }

        }
        private uint GetId(uint id)
        {
            return id & 0x1FFFFFFFU;
        }

        #region Retry
        protected byte[] previousDatas;
        /// <summary>
        /// 超时重试：当连续帧发送接收超时，设置为true，其余设置为false
        /// </summary>
        protected bool SendImmediately = false;
        protected virtual void ReSend(int retryCount)
        {
            //DebugLog?.Invoke($"{CurrentStep} Retry.");
            //await Task.Delay(CurrentTimeout);
            //return;
            if (previousDatas != null && previousDatas.Length > 0)
            {
                DebugLogger($"{CurrentStep} Retry Send.");
                SendFrames(previousDatas);

                if (SendImmediately)
                {
                    DebugLogger($"{CurrentStep} Retry SendContinue.");
                    SendContinueFrames();
                }
                    
            }
        }

        protected void WaitCache()
        {
            DebugLogger($"{CurrentStep} Wait.");
            //await Task.Delay(1000);
            Thread.Sleep(1000);
        }
        #endregion

    }
}