using ERad5TestGUI.Devices;
using ERad5TestGUI.Helpers;
using ERad5TestGUI.Models;
using ERad5TestGUI.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace ERad5TestGUI.Stores
{
    public class SignalStore
    {
        private const string UDSConfigFilePath = @"./Config/UDSConfig.xml";
        /// <summary>
        /// Use AddSignal Function
        /// </summary>
        private readonly List<SignalBase> _signals;
        private readonly List<Signal> _dbcSignals = new List<Signal>();
        private readonly LogService logService;
        private UDS.UDSConfig _udsConfig;
        //public const double AnalogConst = 5 / 4096;
        [Obsolete]
        public DbcFile DbcFile { get; private set; }
        public UDS.UDSConfig UDSConfig { get => _udsConfig ?? (_udsConfig = XmlHelper.DeserializeFromXml<UDS.UDSConfig>(UDSConfigFilePath)); }
        public List<DbcFile> DbcFiles { get; } = new List<DbcFile>();
        public SignalStore(Services.LogService logService)
        {
            this.logService = logService;
            _signals = new List<SignalBase>();
            LoadMsgStates();
            LoadUDSConfig();
            ValueTable.LoadTables();
            LoadDBC();
            LoadSignalLocator();
            //LoadAnalogSignals();
            //LoadAnalogSignals(0x605, nameof(ViewModels.ResolverViewModel));
            //LoadPPAWL(0x01, nameof(ViewModels.PPAWLViewModel));
            //LoadDiscretes(nameof(ViewModels.DiscreteViewModel));
            //LoadDiscreteFixedSignals(nameof(ViewModels.DiscreteViewModel));
            //LoadSavingLogicSignals();
            LoadGDICStatusSignals();
            //LoadPulseInSignals(ViewModels.PulseInViewModel.VIEWNAME);
            //LoadPulseInSignals(0x20, nameof(ViewModels.PPAWLViewModel));
            //LoadPulseOutSignals(nameof(ViewModels.PPAWLViewModel));
            LoadPulseOutFixedSignals();
            LoadLinSignals();
            LoadDiConnectSignals();
            LoadSPISignals();
        }

        ~SignalStore()
        {
            try
            {
                MessagesStates.ForEach(x => x.IsStart = false);

                SignalLocation.Signals = SignalLocation.Signals.Distinct().ToList();
                XmlHelper.SerializeToXml(SignalLocation, SignalLocatorFilePath);
            }
            catch (Exception ex)
            {
                //throw;
                Console.WriteLine(ex.Message);
            }
        }

        public IEnumerable<Signal> DBCSignals { get => _dbcSignals; }

        public IEnumerable<SignalBase> Signals => _signals;

        public List<CANMessage> Messages { get; } = new List<CANMessage>();
        public List<MessageReceiveState> MessagesStates { get; } = new List<MessageReceiveState>();

        private void LoadMsgStates()
        {
            MessagesStates.Add(new MessageReceiveState("TCAN1145_DEVCAN", 0x610, 0x620)
            {
                IsEnable = false,
            });
            MessagesStates.Add(new MessageReceiveState("TCAN1145_CANFD5", new List<uint>() { 0x4FF}));
            MessagesStates.Add(new MessageReceiveState("TCAN1145_CANFD16", 
                new List<uint>() 
                { 
                    0xCA,0x5D0,0x5D3,0x605,0x6F8,0xDB,0x5D4,0x5d7,
                    0x606,0x6F9,0x5c4,0x5c6,0x5c8,0x5c9,0x5ca,0x5cb
                }));
            MessagesStates.ForEach(x => x.Start());
        }

        private void LoadUDSConfig()
        {
            _udsConfig = XmlHelper.DeserializeFromXml<UDS.UDSConfig>(UDSConfigFilePath);
        }

        public void AddSignal(SignalBase signal)
        {
            var updateSignal = DBCSignals.FirstOrDefault(x => x.SignalName == signal.Name);
            if (updateSignal != null)
            {
                signal.UpdateFormDBC(updateSignal);
            }

            if (Signals.FirstOrDefault(x => x.MessageID == signal.MessageID && x.Name == signal.Name) == null)
            {
                _signals.Add(signal);
#if DEBUG
                logService.Debug($"add Signal {signal.Name}");
#endif
                if (this.Messages.FirstOrDefault(x => x.MessageID == signal.MessageID) == null)
                {
                    //create new CanMessag
                    CANMessage message = new CANMessage(signal.MessageID, 64, 0);
                    Messages.Add(message);
                }
                if (SignalLocation.Signals.FirstOrDefault(x => x.MessageID == signal.MessageID && x.Name == signal.Name) == null &&
                    !(signal is DiscreteInputSignal))
                    SignalLocation.Signals.Add(signal);
            }
        }
        /// <summary>
        /// 根据多个ViewName，信号类型，输入/输出获取信号列表
        /// </summary>
        /// <typeparam name="TSignal">信号类型</typeparam>
        /// <param name="inOrOut">输入/输出</param>
        /// <param name="viewNames"></param>
        /// <returns></returns>
        public IEnumerable<TSignal> GetSignals<TSignal>(bool? inOrOut, params string[] viewNames) where TSignal : SignalBase
        {
            List<TSignal> signals = new List<TSignal>();
            foreach (var viewName in viewNames)
            {
                signals.AddRange(GetSignals<TSignal>(viewName, inOrOut));
            }

            return signals.Distinct();
        }
        /// <summary>
        /// 根据ViewName，信号类型，输入/输出获取信号列表
        /// </summary>
        /// <typeparam name="TSignal"></typeparam>
        /// <param name="viewName">为空时，获取所有信号</param>
        /// <param name="inOrOut">null:不区分；false:In;true:Out</param>
        /// <returns></returns>
        public IEnumerable<TSignal> GetSignals<TSignal>(string viewName = "", bool? inOrOut = null) where TSignal : SignalBase
        {
            viewName = SignalBase.ReplaceViewModel(viewName);
            _signals.Sort((x, y) =>
            {
                if (y.MessageID > x.MessageID)
                {
                    return -1;
                }
                else if (y.MessageID == x.MessageID)
                {
                    return x.StartBit.CompareTo(y.StartBit);
                }
                else
                    return 1;
            });

            return _signals.OfType<TSignal>()
                           .Where(x =>
                           {
                               if (inOrOut.HasValue)
                               {
                                   if (inOrOut.Value == x.InOrOut)
                                   {
                                       if (string.IsNullOrEmpty(viewName))
                                           return true;
                                       return x.ViewName.IndexOf(viewName, StringComparison.OrdinalIgnoreCase) > -1;
                                   }
                                   else
                                   {
                                       return false;
                                   }
                               }
                               else
                               {
                                   if (string.IsNullOrEmpty(viewName))
                                       return true;
                                   return x.ViewName.IndexOf(viewName, StringComparison.OrdinalIgnoreCase) > -1;
                               }
                           });
        }
        /// <summary>
        /// 模糊查询信号，若没有则新建信号
        /// </summary>
        /// <typeparam name="TSignal"></typeparam>
        /// <param name="signalName"></param>
        /// <param name="inOrOut"></param>
        /// <param name="addToStore">新建信号是否加入全局</param>
        /// <returns></returns>
        public IEnumerable<TSignal> GetSignalsByName<TSignal>(string signalName, bool inOrOut = false, bool addToStore = true) where TSignal : SignalBase, new()
        {
            var exsitSignals = _signals.OfType<TSignal>().Where(x => x.Name.IndexOf(signalName, StringComparison.OrdinalIgnoreCase) > -1).ToList();

            List<Signal> dbcSignals = new List<Signal>();

            DbcFiles.ForEach(
            file =>
            {
                file.Messages.ForEach(msg =>
                {
                    msg.signals.ForEach(signal =>
                    {
                        if (signal.SignalName.IndexOf(signalName, StringComparison.OrdinalIgnoreCase) > -1 &&
                            exsitSignals.FirstOrDefault(eS => eS.Name == signal.SignalName) == null)
                        {
                            dbcSignals.Add(signal);
                        }
                    });
                });
            });

            dbcSignals.ForEach(nS =>
            {
                var t = new TSignal()
                {
                    Name = nS.SignalName,
                    MessageID = nS.MessageID,
                    InOrOut = inOrOut
                };
                t.UpdateFormDBC(nS);
                if (addToStore)
                    AddSignal(t);
                exsitSignals.Add(t);
            });

            return exsitSignals;
        }
        /// <summary>
        /// 模糊查询信号，若没有则新建信号
        /// </summary>
        /// <typeparam name="TSignal"></typeparam>
        /// <param name="signalName"></param>
        /// <param name="inOrOut"></param>
        /// <param name="addToStore"></param>
        /// <returns></returns>
        public TSignal GetSignalByName<TSignal>(string signalName, bool inOrOut = false, bool addToStore = true) where TSignal : SignalBase, new()
        {
            var s = _signals.OfType<TSignal>().FirstOrDefault(x => x.Name == signalName);

            if (s != null)
                return s;

            Signal dbcSignal = null;
            DbcFiles.ForEach(
            file =>
            {
                file.Messages.ForEach(x =>
                {
                    if (dbcSignal == null)
                        dbcSignal = x.signals.FirstOrDefault(ss => ss.SignalName == signalName);
                });
            });
            if (dbcSignal != null)
            {
                var t = new TSignal()
                {
                    Name = signalName,
                    MessageID = dbcSignal.MessageID,
                    InOrOut = inOrOut
                };
                t.UpdateFormDBC(dbcSignal);
                if (addToStore)
                    AddSignal(t);
                return t;
            }

            return default;
        }

        public ObservableCollection<TSignal> GetObservableCollection<TSignal>(string viewName = "") where TSignal : SignalBase
        {
            viewName = SignalBase.ReplaceViewModel(viewName);
            ObservableCollection<TSignal> signals = new ObservableCollection<TSignal>();
            _signals.Sort((x, y) =>
            {
                if (y.MessageID > x.MessageID)
                {
                    return -1;
                }
                else if (y.MessageID == x.MessageID)
                {
                    return x.StartBit.CompareTo(y.StartBit);
                }
                else
                    return 1;
            });
            foreach (var item in _signals.OfType<TSignal>())
            {
                if (string.IsNullOrEmpty(viewName))
                    signals.Add(item);
                else if (item.ViewName.IndexOf(viewName, StringComparison.OrdinalIgnoreCase) > -1)
                    signals.Add(item);
            }

            return signals;
        }

        #region DBC Parse
        public IEnumerable<SignalBase> ParseMsgsYield(IEnumerable<IFrame> can_msg)
        {
            return ParseMsgsYield(can_msg, this.Signals);
        }

        /// <summary>
        /// 解析CAN报文 DBC 信号
        /// </summary>
        /// <param name="can_msg"></param>
        /// <param name="singals"></param>
        /// <returns>signal or NULL</returns>
        public IEnumerable<SignalBase> ParseMsgsYield(IEnumerable<IFrame> can_msg, IEnumerable<SignalBase> singals)
        {
            //Dictionary<SignalItem, string> signalValue = new Dictionary<SignalItem, string>();
            //解析can msg
            // 为每一个 signal 执行以下逻辑：
            foreach (var baseSignal in singals)
            {
                SignalBase signal = baseSignal as SignalBase;
                // 若信号无需收发，则跳过此次循环。
                //if (!signal.WhetherSendOrGet)
                //    continue;
                // 获取 can_msg 中 messageID 与当前信号相符的 CANReceiveFrame 元素集合，若不存在则直接返回当前 signal（yield return）。
                var canThisID = can_msg.Where(x => x.MessageID == signal.MessageID);
                if (canThisID != null && canThisID.Count() != 0)
                {
                    // 对于查找到匹配 CAN 消息中包含当前 Signal Item 的情况进行如下处理：
                    foreach (var item in canThisID)
                    {
                        var val = ParseBytes(item.Data, signal);

                        signal.OriginValue = val;
                        //signal.TimeStamp = (int)item.TimeStampInt;
                        yield return signal;

                    }
                }
                else
                {
                    yield return null;
                }
            }
            //return signalValue;
        }
        /// <summary>
        /// 解析单个ID 报文
        /// </summary>
        /// <param name="can_msg"></param>
        /// <param name="singals"></param>
        /// <returns>DBCSignal</returns>
        public IEnumerable<SignalBase> ParseMsgYield(IFrame can_msg, IEnumerable<SignalBase> singals)
        {
            foreach (var signal in singals)
            {
                if (signal.MessageID != can_msg.MessageID)
                    yield return signal;

                var val = ParseBytes(can_msg.Data, signal);
                signal.OriginValue = val;
                //signal.TimeStamp = (int)item.TimeStampInt;
                yield return signal;
            }
        }

        public double ParseBytes(byte[] msg, SignalBase signal)
        {
            int startBit = signal.StartBit;

            int startIndex = startBit / 8;
            int startOffset = startBit % 8;

            int rawValue = 0;

            if (signal.ByteOrder == 0)//moto
            {
                for (int i = 0; i < signal.Length; i++)
                {
                    int byteIndex = startIndex + (7 - startOffset + i) / 8;
                    int bitIndex = (GetMotorolaBitIndex(startOffset - i) % 8);
                    //0 % 8 = 0;1

                    int bitvaule = (msg[byteIndex] >> bitIndex) & 1;
                    rawValue = (rawValue << 1) | (bitvaule);
                }
            }
            else
            {
                for (int i = 0; i < signal.Length; i++)
                {
                    int byteIndex = startIndex + (startOffset + i) / 8;
                    int bitIndex = (startOffset + i) % 8;
                    if (byteIndex >= msg.Length)
                    {
                        logService.Warn($"{signal.MessageID:X} Can Msg Length Error:${signal.Name}.Startbit bigger than can msg");
                        continue;
                    }
                    if ((msg[byteIndex] & (1 << bitIndex)) != 0)
                    {
                        rawValue |= (1 << i);
                    }
                }
            }

            double physicalValue = rawValue * signal.Factor + signal.Offset;
            signal.OriginValue = physicalValue;
            return physicalValue;

        }

        public IEnumerable<IFrame> BuildFrames(IEnumerable<SignalBase> signals)
        {
            List<IFrame> cAN_Msg_BytesList = new List<IFrame>();

            var ids = signals.Select(x => x.MessageID).Distinct();
            var allidsignals = _signals.Where(x => ids.Contains(x.MessageID));
            foreach (var signal in allidsignals)
            {
                ModifyBytesRef(signal);
            }

            foreach (var messageid in ids)
            {
                // var bitsdata =
                var message = Messages.FirstOrDefault(x => x.MessageID == messageid);
                if (message != null)
                {
                    byte[] resData = message.Data;//8 帧数据
                    cAN_Msg_BytesList.Add(new CanFrame(messageid, resData, FrameFlags.CANFDSpeed));
                }
            }

            return cAN_Msg_BytesList;
        }

        private void ModifyBytesRef(SignalBase signal)
        {
            var message = Messages.FirstOrDefault(x => x.MessageID == signal.MessageID);
            if (message != null)
            {
                byte[] initdata = message.Data;

                if (signal.StartBit / 8 > initdata.Length)
                    throw new ArgumentOutOfRangeException("信号Startbit错误，检查是否CANFD！");

                ReConstructByteArray(signal, ref initdata);
                message.Data = initdata;
            }
        }

        public void ReConstructByteArray(SignalBase signal, ref byte[] msg)
        {
            int startBit = signal.StartBit;

            int startIndex = startBit / 8;
            int startOffset = startBit % 8;

            int rawvalue = (int)(((decimal)(signal.OriginValue) - (decimal)signal.Offset) / (decimal)signal.Factor);

            if (signal.ByteOrder == 0)//moto
            {
                for (int i = 0; i < signal.Length; i++)
                {
                    int byteIndex = startIndex + (7 - startOffset + i) / 8;
                    int bitIndex = (GetMotorolaBitIndex(startOffset - i) % 8);
                    int clearmask = ~(1 << bitIndex);
                    msg[byteIndex] = (byte)(msg[byteIndex] & clearmask);
                    int bitvaule = (rawvalue >> (signal.Length - i - 1)) & 1;//(msg[byteIndex] >> bitIndex) & 1;
                    msg[byteIndex] = (byte)(msg[byteIndex] | (bitvaule << bitIndex));
                }
            }
            else
            {
                for (int i = 0; i < signal.Length; i++)
                {
                    int byteIndex = startIndex + (startOffset + i) / 8;
                    int bitIndex = (startOffset + i) % 8;
                    int clearmask = ~(1 << bitIndex);
                    msg[byteIndex] = (byte)(msg[byteIndex] & clearmask);
                    if ((rawvalue & (1 << i)) != 0)
                    {
                        msg[byteIndex] |= (byte)(1 << bitIndex);
                    }
                }
            }
        }

        private int GetMotorolaBitIndex(int index)
        {
            while (index < 0)
            {
                index += 8;
            }
            return index;
        }
        #endregion

        /// <summary>
        /// 根据PageName获取 DBC 文件中的信号
        /// </summary>
        /// <param name="viewName"></param>
        /// <returns></returns>
        private List<Signal> GetSignalsByPageName(string viewName)
        {
            List<Signal> signals = new List<Signal>();

            DbcFiles.ForEach(
                file =>
                {
                    file.Messages.ForEach(x =>
                     {
                         //List<AnalogSignal> analogSignals = new List<AnalogSignal>();
                         foreach (var signal in x.signals)
                         {
                             if (!string.IsNullOrEmpty(signal.Page) && signal.Page.IndexOf(viewName, StringComparison.OrdinalIgnoreCase) > -1)
                             {
                                 signal.MessageID = x.MessageID;
                                 signal.MessageName = x.messageName;
                                 signals.Add(signal);
                             }
                         }
                     });
                });

            return signals;
        }
        /// <summary>
        /// 获取信号枚举
        /// </summary>
        /// <param name="msgID"></param>
        /// <param name="signalName"></param>
        /// <returns>Dictionary&lt;int,string&gt;</returns>
        public Dictionary<int, string> GetKeyValuePairs(uint msgID, string signalName)
        {
            var enumValue = DbcFiles.Select(x =>
            {
                if (x.ValEnums.FirstOrDefault(e => e.messageID == msgID && e.signalName == signalName) != null)
                    return x.ValEnums.FirstOrDefault(e => e.messageID == msgID && e.signalName == signalName);
                return null;
            });

            return enumValue.FirstOrDefault().values;
        }

        private List<Signal> GetSignalsByID(uint msgId)
        {
            List<Signal> signals = new List<Signal>();

            DbcFiles.ForEach(
                file =>
                {
                    file.Messages.ForEach(msg =>
                    {
                        if(msg.MessageID == msgId)
                        {
                            signals.AddRange(msg.signals);
                        }
                    });
                });

            return signals;
        }

        private void LoadDBC()
        {
            var files = Directory.GetFiles(@".\Config", "*.dbc");
            foreach (var item in files)
            {
                var dbcf = new DbcFile(item);
                dbcf.Messages.ForEach(x =>
                {
                    foreach (var signal in x.signals)
                    {
                        signal.MessageID = x.MessageID;
                        signal.MessageName = x.messageName;
                        _dbcSignals.Add(signal);
                    }
                });
                DbcFiles.Add(dbcf);
            }
        }

        private void LoadAnalogSignals(string viewName = nameof(ViewModels.AnalogViewModel))
        {
            viewName = SignalBase.ReplaceViewModel(viewName);
            var analogViewSignals = GetSignalsByPageName(viewName);
            analogViewSignals.ForEach(signal =>
            {
                if (signal.Comment.KeyValues.TryGetValue("Page", out string pageName) && pageName.IndexOf(viewName) > -1)
                {
                    AnalogSignal analogSignal = new AnalogSignal(signal, viewName);
                    SaveViewSignalLocator(viewName, analogSignal);
                }
            });
        }

        private void LoadDiscretes(string viewName)
        {
            //input
            viewName = SignalBase.ReplaceViewModel(viewName);
            var discreteViewSignals = GetSignalsByPageName(viewName);
            var outDiscreteSignals = discreteViewSignals.Where(x => x.InOrOut == false);

            //var analogViewSignals = GetSignalsByPageName(viewName);
            outDiscreteSignals.ToList().ForEach(signal =>
            {
                DiscreteOutputSignal analogSignal = new DiscreteOutputSignal(signal, viewName);
                //find a state 

                var signalState = DBCSignals.FirstOrDefault(x => x.SignalName == signal.SignalName + "_State");
                if (signalState != null)
                {
                    var existInput = Signals.FirstOrDefault(x => x.Name == signal.SignalName + "_State");
                    if (existInput != null && existInput is DiscreteInputSignal exsitDiscrete)
                    {
                        analogSignal.State = exsitDiscrete;
                    }
                    else
                    {
                        var input = new DiscreteInputSignal(signalState, viewName);
                        analogSignal.State = input;
                        AddSignal(input);
                    }
                }
                else
                {
                    return;
                }

                SaveViewSignalLocator(viewName, analogSignal);
                AddSignal(analogSignal);
            });

            var inputs = discreteViewSignals.Where(x => !Signals.Any(y => x.SignalName == y.Name || x.SignalName == y.Name + "_State"));

            inputs.ToList().ForEach(signal =>
            {
                DiscreteInputSignal analogSignal = new DiscreteInputSignal(signal, viewName);
                if (string.IsNullOrEmpty(analogSignal.PinNumber))
                    return;
                SaveViewSignalLocator(viewName, analogSignal);
                AddSignal(analogSignal);
            });
        }

        private void LoadDiscreteFixedSignals(string viewName)
        {
            viewName = SignalBase.ReplaceViewModel(viewName);
            AddSignal(new DiscreteOutputSignal(DBCSignals.FirstOrDefault(x => x.SignalName == "SEND_BAD_ANSWER"), viewName));
            AddSignal(new DiscreteOutputSignal(DBCSignals.FirstOrDefault(x => x.SignalName == "DIS_SBC_WWD_TRIG"), viewName));
            AddSignal(new DiscreteOutputSignal(DBCSignals.FirstOrDefault(x => x.SignalName == "FD5_INH_DISABLE"), viewName));
            AddSignal(new DiscreteOutputSignal(DBCSignals.FirstOrDefault(x => x.SignalName == "FD16_INH_DISABLE"), viewName));
        }

        private void LoadPPAWL(uint msgid, string viewName)
        {
            int startbit = 0;
            viewName = SignalBase.ReplaceViewModel(viewName);
            GenerateVirtualSignals<AnalogSignal>(ref msgid, viewName, ref startbit, signalLength: 12);
            GenerateVirtualSignals<DiscreteInputSignal>(ref msgid, viewName, ref startbit, signalLength: 1);
            GenerateVirtualSignals<DiscreteOutputSignal>(ref msgid, viewName, ref startbit, signalLength: 1);
        }

        private void LoadSavingLogicSignals()
        {
            for (int i = 0; i < 4; i++)
            {
                //input
                int j;
                for (j = 0; j < 7; j++)
                {
                    _signals.Add(new SavingLogicSignal()
                    {
                        Name = $"SavingLogicSignal{i}",
                        GroupName = $"level {i + 1}",
                        InputOrOutput = true,
                        MessageID = 0x102,
                        StartBit = i + j,
                        Length = 1,
                        Factor = 1,
                        Offset = 0,
                        ByteOrder = 1
                    });
                }
                //output
                for (j = 7; j < 15; j++)
                {
                    _signals.Add(new SavingLogicSignal()
                    {
                        Name = $"SavingLogicSignal{i}",
                        GroupName = $"level {i + 1}",
                        InputOrOutput = false,
                        MessageID = 0x102,
                        StartBit = i + j,
                        Length = 1,
                        Factor = 1,
                        Offset = 0,
                        ByteOrder = 1
                    });
                }
            }

            //pins output
            for (int i = 0; i < 10; i++)
            {
                _signals.Add(new SavingLogicButtonSignal()
                {
                    Name = $"Pins_output {i}",
                });
            }
        }

        private void LoadGDICStatusSignals()
        {
            //load GDIC Status
            //viewName = SignalBase.ReplaceViewModel(viewName);
            var gdicStatusSignals = GetSignalsByPageName("GDIC3162_Status");
            foreach (var signal in gdicStatusSignals)
            {
                if (signal.SignalName.IndexOf("status", StringComparison.OrdinalIgnoreCase) > -1
                    || signal.SignalName.IndexOf("statcon", StringComparison.OrdinalIgnoreCase) > -1
                    || signal.SignalName.IndexOf("register", StringComparison.OrdinalIgnoreCase) > -1)
                {
                    bool inOrOut = signal.SignalName.IndexOf("write", StringComparison.OrdinalIgnoreCase) > -1;
                    GDICStatusDataSignal statusDataSignal = new GDICStatusDataSignal(signal, "GDIC")
                    {
                        InOrOut = inOrOut
                    };
                    SaveViewSignalLocator("GDIC", statusDataSignal);
                }
            }
            //load GDIC Aout Signal
            var gdicAoutSignals = GetSignalsByPageName("GDIC3162_Aout");
            foreach (var signal in gdicAoutSignals)
            {
                var aout = new GDICAoutSignal(signal, "GDIC");
                if (signal.SignalName.IndexOf("select", StringComparison.OrdinalIgnoreCase) > -1)
                {
                    aout.InOrOut = true;
                }
                SaveViewSignalLocator("GDIC", aout);
            }

            var gdicResSignals = GetSignalsByPageName("GDIC3162_Registers");
            foreach (var signal in gdicResSignals)
            {
                var res = new GDICRegisterSignal(signal, ViewModels.GDICViewModel.GDICRegisterViewName);

                SaveViewSignalLocator(ViewModels.GDICViewModel.GDICRegisterViewName, res);
            }

            var gdicAdcSignals = GetSignalsByPageName("GDIC3162_ADC");
            foreach (var signal in gdicAdcSignals)
            {
                var res = new GDICRegisterSignal(signal, ViewModels.GDICViewModel.GDICADCViewName);
                if (signal.SignalName.IndexOf("sel", StringComparison.OrdinalIgnoreCase) > -1)
                {
                    res.InOrOut = true;
                }

                SaveViewSignalLocator(ViewModels.GDICViewModel.GDICADCViewName, res);
            }
            //Top-U
            //GenerateGDICSignals("Top-U");
            //GenerateGDICSignals("Top-V");
            //GenerateGDICSignals("Top-W");
            //GenerateGDICSignals("Bot-U");
            //GenerateGDICSignals("Bot-V");
            //GenerateGDICSignals("Bot-W");
        }

        private void GenerateGDICSignals(string groupName)
        {
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    GDICStatusDataSignal gDICStatusSignal = new GDICStatusDataSignal($"{groupName} Status-{i + 1}")
                    {
                        Name = $"Data{j}"
                    };
                    _signals.Add(gDICStatusSignal);
                }
            }
        }

        private void LoadPulseInSignals(string viewName)
        {
            viewName = SignalBase.ReplaceViewModel(viewName);
            var pulseInViewSignals = GetSignalsByPageName(viewName);

            pulseInViewSignals.ForEach(signal =>
            {
                if (signal.SignalName.IndexOf("_Duty") > -1 || signal.SignalName.IndexOf("_Freq") > -1)
                {
                    string[] groupName = signal.SignalName.Split(new string[] { "_Duty", "_Freq" }, StringSplitOptions.RemoveEmptyEntries);
                    if (groupName.Length == 1)
                    {
                        PulseInSignal pulseInSignal = new PulseInSignal(signal, signal.Page.Replace(',', ';'), groupName[0]);
                        //{
                        //    Name = signal.SignalName,
                        //    StartBit = (int)signal.startBit,
                        //    Factor = signal.factor,
                        //    Offset = signal.offset,
                        //    ByteOrder = (int)signal.byteOrder,
                        //    Length = (int)signal.signalSize,
                        //    MessageID = signal.MessageID,
                        //    ViewName = signal.Page.Replace(',',';')
                        //};
                        SaveViewSignalLocator(viewName, pulseInSignal);
                    }
                }
            });
            //int startbit = 0;
            //GeneratePulseInSignals(viewName, 12, ref msgID, ref startbit);
        }

        private void GeneratePulseInSignals(string viewName, int signalLength, ref uint msgID, ref int startbit)
        {

            for (int i = 0; i < 10; i++)
            {
                startbit = signalLength * 2 * i;
                if (startbit + signalLength * 2 > 64 * 8 - 1)
                {
                    startbit = 0;
                    msgID += 1;
                }
                PulseInSignal signal_dc = new PulseInSignal($"{viewName}.PulseInSignal {i}")
                {
                    Name = "DC",
                    MessageID = msgID,
                    StartBit = startbit,
                    Length = signalLength,
                    Factor = 1,
                    Offset = 0,
                    ByteOrder = 1,
                    ViewName = viewName,
                };
                PulseInSignal signal_Freq = new PulseInSignal($"{viewName}.PulseInSignal {i}")
                {
                    Name = "Freq",
                    MessageID = msgID,
                    StartBit = startbit + signalLength,
                    Length = signalLength,
                    Factor = 1,
                    Offset = 0,
                    ByteOrder = 1,
                    ViewName = viewName,
                };
                _signals.Add(signal_dc);
                _signals.Add(signal_Freq);
            }
        }

        private void LoadPulseOutSignals(string viewName)
        {
            viewName = SignalBase.ReplaceViewModel(viewName);
            var pulseOutViewSignals = GetSignalsByPageName(viewName);
        }

        private void LoadPulseOutFixedSignals()
        {
            string viewName = SignalBase.ReplaceViewModel(ViewModels.PulseOutViewModel.VIEWNAME);
            var pwm_U_Duty = new PulseOutSingleSignal(DBCSignals.FirstOrDefault(x => x.SignalName == "PWM_U_Duty"), viewName);
            AddSignal(pwm_U_Duty);
            var pwm_V_Duty = new PulseOutSingleSignal(DBCSignals.FirstOrDefault(x => x.SignalName == "PWM_V_Duty"), viewName);
            AddSignal(pwm_V_Duty);
            var pwm_W_Duty = new PulseOutSingleSignal(DBCSignals.FirstOrDefault(x => x.SignalName == "PWM_W_Duty"), viewName);
            AddSignal(pwm_W_Duty);
            var uvm_PWM_Freq = new PulseOutSingleSignal(DBCSignals.FirstOrDefault(x => x.SignalName == "UVW_PWM_Freq"), viewName)
            {
                OriginValue = 10000
            };
            AddSignal(uvm_PWM_Freq);
        }

        private void LoadLinSignals()
        {
            string viewName = "LIN";
            var lINViewSignals = GetSignalsByPageName(viewName);

            foreach (var item in lINViewSignals)
            {
                var linConfigSignal = new LinConfigSignal(item, viewName);
                //Receive Data is IN
                if (linConfigSignal.Name.IndexOf("Receive_Data") < 0)
                {
                    linConfigSignal.InOrOut = true;
                }
                //else
                //{
                //    linConfigSignal.InOrOut = true;
                //}
                AddSignal(linConfigSignal);
            }
        }

        private void GenerateVirtualSignals<TSignal>(ref uint id, string viewName, ref int startbit, int count = 10, int signalLength = 1)
          where TSignal : SignalBase, new()
        {

            for (int i = 0; i < count; i++)
            {
                startbit += signalLength * i;
                if (startbit + signalLength > 64 * 8 - 1)
                {
                    startbit = 0;
                    id += 1;
                }
                _signals.Add(new TSignal()
                {
                    Name = $"{viewName}.{typeof(TSignal).Name}{i}",
                    MessageID = id,
                    StartBit = signalLength * i,
                    Length = signalLength,
                    Factor = 1,
                    Offset = 0,
                    ByteOrder = 1,
                    ViewName = viewName,
                });
            }
        }

        private void LoadDiConnectSignals()
        {
            //throw new NotImplementedException();
            var id0x25 = GetSignalsByID(0x25);
            foreach (var item in id0x25)
            {
                AddSignal(new SignalBase(item, "DisConnect"));
            }
            var id0x75 = GetSignalsByID(0x75);
            foreach (var item in id0x75)
            {
                AddSignal(new SignalBase(item, "DisConnect"));
            }
            var id0x15 = GetSignalsByID(0x15);
            foreach (var item in id0x15)
            {
                AddSignal(new SignalBase(item, "DisConnect")
                {
                    InOrOut = true
                });
            }
            var id0x16 = GetSignalsByID(0x16);
            foreach (var item in id0x16)
            {
                AddSignal(new SignalBase(item, "DisConnect")
                {
                    InOrOut = true
                });
            }
        }

        private void LoadSPISignals()
        {
            string viewName = "SPI";
            var spiViewSignals = GetSignalsByPageName(viewName);

            foreach (var item in spiViewSignals)
            {
                if (item.SignalName.IndexOf("Select") > -1)
                {
                    SPISignal spisignal = new SPISignal(item);
                    spisignal.InOrOut = true;
                    AddSignal(spisignal);
                    var keys = GetKeyValuePairs(item.MessageID, item.SignalName);
                    if (keys != null)
                    {
                        foreach (var kv in keys)
                        {
                            if (!SPIValueTable.Value2Baudrate.ContainsKey(kv.Key))
                                SPIValueTable.Value2Baudrate.Add(kv.Key, kv.Value);
                        }
                    }
                }
                else if (item.SignalName.IndexOf("Current") > -1)
                {
                    SPISignal spisignal = new SPISignal(item);
                    //spisignal.InOrOut = true;
                    AddSignal(spisignal);
                }
                else
                {
                    DiscreteOutputSignal spisignal = new DiscreteOutputSignal(item, "SPI");
                    spisignal.InOrOut = true;
                    //spisignal.FixedOut = true;
                    AddSignal(spisignal);
                }
            }
        }

        #region Signal Locator
        [Obsolete]
        public ViewsSignals SignalLocatorInfo { get; private set; }
        public SignalCollection SignalLocation { get; private set; }
        private const string SignalLocatorFilePath = @"Config/SignalLocator.xml";
        //private const string SignalLocatorFilePath2 = @"Config/SignalLocator2.xml";
        private void LoadSignalLocator()
        {
            SignalLocation = XmlHelper.DeserializeFromXml<SignalCollection>(SignalLocatorFilePath);
            if (SignalLocation == null)
            {
                SignalLocation = new SignalCollection();
            }

            foreach (var signal in SignalLocation.Signals)
            {
                AddSignal(signal);

                if (signal is DiscreteOutputSignal disout)
                {
                    if (disout.State != null)
                    {
                        AddSignal(disout.State);
                        if (disout.State.Name.Contains("State"))
                        {
                            disout.IsOutput = true;
                        }
                    }
                }
            }
        }

        public void SaveViewSignalLocator(string viewName, SignalBase signal)
        {
            //var viewLocator = SignalLocatorInfo.GetViewSignalInfo(viewName);
            if (!SignalLocation.Signals.Contains(signal))
            {
                SignalLocation.Signals.Add(signal);
                AddSignal(signal);
            }
            else
            {
                var exsit = SignalLocation.Signals.FirstOrDefault(x => x.Name == signal.Name && x.MessageID == signal.MessageID);
                exsit.ViewName = exsit.ViewName + viewName;
            }
        }
        public void SaveViewSignalLocator(string viewName, IEnumerable<SignalBase> signals, bool clear = true)
        {
            viewName = SignalBase.ReplaceViewModel(viewName);
            //var viewLocator = SignalLocatorInfo.GetViewSignalInfo(viewName);
            if (clear)
                SignalLocation.Signals.RemoveAll(x => x.ViewNames.Contains(viewName));
            SignalLocation.Signals.AddRange(signals);
        }
        //public void SaveSignalLocator()
        //{
        //    XmlHelper.SerializeToXml(SignalLocatorInfo, SignalLocatorFilePath);
        //}
        /// <summary>
        /// Delete ViewInfo,only Signal List
        /// </summary>
        [Obsolete]
        public void LocatorToLocation()
        {
            SignalLocatorInfo.ViewSignalsInfos.ForEach(viewInfo =>
            {
                if (SignalLocation == null) { SignalLocation = new SignalCollection(); }

                SignalLocation.Signals.AddRange(viewInfo.Signals);
            });
        }
        #endregion

        public List<SignalValueTable> SignalValueTables { get; private set; }

        private void GenerateSignalValueTables()
        {
            SignalValueTables = new List<SignalValueTable>();
            var table = new SignalValueTable()
            {
                TableName = "Table1",
                Rows = new List<SignalValueTableRow>()
            };
            table.Rows.Add(new SignalValueTableRow(-40, "5*Rt/(Rt+20k)", 217.51));
            table.Rows.Add(new SignalValueTableRow(-39, "5*Rt/(Rt+20k)", 205.26));
            SignalValueTables.Add(table);

            var table3 = new SignalValueTable()
            {
                TableName = "Table3",
                Rows = new List<SignalValueTableRow>()
            };
            table.Rows.Add(new SignalValueTableRow(-40, "5*Rt/(Rt+20k)", 217.51));
            table.Rows.Add(new SignalValueTableRow(-39, "5*Rt/(Rt+20k)", 205.26));
            SignalValueTables.Add(table3);
        }

        public double ConvertByTable(string tableName, double origionalValue)
        {
            var table = SignalValueTables.FirstOrDefault(x => x.TableName == tableName);

            if (table == null)
                return origionalValue;

            return table.GetTForV(origionalValue);
        }
    }

    public static class ValueTable
    {
        public static string Path = @"Config/ValueTables.xml";
        private static List<SignalValueTable> signalValueTables;
        public static List<SignalValueTable> SignalValueTables { get => signalValueTables; }

        public static void LoadTables()
        {
            signalValueTables = XmlHelper.DeserializeFromXml<List<SignalValueTable>>(Path);
        }

        public static double ConvertByTable(string tableName, double origionalValue)
        {
            var table = SignalValueTables.FirstOrDefault(x => x.TableName == $"{tableName}");

            if (table == null)
                return origionalValue;

            return table.GetTForV(origionalValue);
        }
    }

   
}
