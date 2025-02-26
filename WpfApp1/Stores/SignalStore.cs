using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Serialization;
using WpfApp1.Devices;
using WpfApp1.Helpers;
using WpfApp1.Models;
using WpfApp1.Services;

namespace WpfApp1.Stores
{
    public class SignalStore
    {
        private readonly List<SignalBase> _signals;
        private readonly List<Signal> _dbcSignals = new List<Signal>();
        private readonly LogService logService;
        //public const double AnalogConst = 5 / 4096;
        public DbcFile DbcFile { get; private set; }
        public SignalStore(Services.LogService logService)
        {
            this.logService = logService;
            _signals = new List<SignalBase>();
            ValueTable.LoadTables();
            LoadDBC();
            LoadSignalLocator();
            //LoadAnalogSignals();
            //LoadAnalogSignals(0x605, nameof(ViewModels.ResolverViewModel));
            //LoadPPAWL(0x01, nameof(ViewModels.PPAWLViewModel));
            //LoadDiscretes(nameof(ViewModels.DiscreteViewModel));
            //LoadSavingLogicSignals();
            LoadGDICStatusSignals();
            //LoadPulseInSignals(0x10, (ViewModels.PulseInViewModel.VIEWNAME));
            //LoadPulseInSignals(0x20, nameof(ViewModels.PPAWLViewModel));
            //LoadPulseOutSignals(nameof(ViewModels.PPAWLViewModel));
            LoadPulseOutFixedSignals();
        }

       

        ~SignalStore()
        {
            try
            {
                SignalLocation.Signals.Distinct();
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

        public void AddSignal(SignalBase signal)
        {
            var updateSignal = DBCSignals.FirstOrDefault(x => x.SignalName == signal.Name);
            if (updateSignal != null && updateSignal.MessageID != signal.MessageID)
            {
                logService.Debug($"Signal MsgID Update {signal.MessageID:X}->{updateSignal.MessageID:X}");
                signal.MessageID = updateSignal.MessageID;
            }

            if (updateSignal != null && !string.IsNullOrEmpty(updateSignal.Unit))
            {
                signal.Unit = updateSignal.Unit;
            }

            if (Signals.FirstOrDefault(x => x.MessageID == signal.MessageID && x.Name == signal.Name) == null)
            {
                _signals.Add(signal);
                logService.Debug($"add Signal {signal.Name}");
                if (this.Messages.FirstOrDefault(x => x.MessageID == signal.MessageID) == null)
                {
                    //create new CanMessag
                    CANMessage message = new CANMessage(signal.MessageID, 64, 0);
                    Messages.Add(message);
                }
            }
        }

        public IEnumerable<TSignal> GetSignals<TSignal>(params string[] viewNames) where TSignal : SignalBase
        {
            List<TSignal> signals = new List<TSignal>();
            foreach (var viewName in viewNames)
            {
                signals.AddRange(GetSignals<TSignal>(viewName));
            }

            return signals.Distinct();
        }
        public IEnumerable<TSignal> GetSignals<TSignal>(string viewName = "") where TSignal : SignalBase
        {
            viewName = SignalBase.ReplaceViewModel(viewName);
            _signals.Sort((x, y) => {
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
                               if (string.IsNullOrEmpty(viewName))
                                   return true;
                               return x.ViewName.IndexOf(viewName) > -1;
                           });
        }

        public ObservableCollection<TSignal> GetObservableCollection<TSignal>(string viewName = "") where TSignal : SignalBase
        {
            viewName = SignalBase.ReplaceViewModel(viewName);
            ObservableCollection<TSignal> signals = new ObservableCollection<TSignal>();
            _signals.Sort((x, y) => {
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
                else if (item.ViewName.IndexOf(viewName) > -1)
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
        /// 
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

            return physicalValue;

        }

        public IEnumerable<IFrame> BuildFrames(IEnumerable<SignalBase> signals)
        {
            List<IFrame> cAN_Msg_BytesList = new List<IFrame>();

            var ids = signals.Select(x => x.MessageID).Distinct();

            foreach (var signal in signals)
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
                    cAN_Msg_BytesList.Add(new CanFrame(messageid, resData));
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

        private List<Signal> GetSignalsByPageName(string viewName)
        {
            List<Signal> signals = new List<Signal>();

            DbcFile.Messages.ForEach(x =>
            {
                //List<AnalogSignal> analogSignals = new List<AnalogSignal>();
                foreach (var signal in x.signals)
                {
                    if (signal.Comment.KeyValues.TryGetValue("Page", out string pageName) && pageName.IndexOf(viewName, StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        signal.MessageID = x.MessageID;
                        signal.MessageName = x.messageName;
                        signals.Add(signal);
                    }
                }
            });

            return signals;
        }

        private void LoadDBC()
        {
            DbcFile = new DbcFile(@".\Config\Erad5_GUI_DEVCAN.dbc");

            DbcFile.Messages.ForEach(x =>
            {
                //List<AnalogSignal> analogSignals = new List<AnalogSignal>();
                foreach (var signal in x.signals)
                {
                    signal.MessageID = x.MessageID;
                    signal.MessageName = x.messageName;
                    _dbcSignals.Add(signal);
                }
            });
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
                DiscreteInputSignal analogSignal = new DiscreteInputSignal(signal,viewName);
                if (string.IsNullOrEmpty(analogSignal.PinNumber))
                    return;
                SaveViewSignalLocator(viewName, analogSignal);
                AddSignal(analogSignal);
            });
        }

        private void LoadPPAWL(uint msgid,string viewName)
        {
            int startbit = 0;
            viewName = SignalBase.ReplaceViewModel(viewName);
            GenerateVirtualSignals<AnalogSignal>(ref msgid, viewName,ref startbit, signalLength: 12);
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

        private void LoadPulseInSignals(uint msgID, string viewName)
        {
            
            viewName = SignalBase.ReplaceViewModel(viewName);
            var pulseInViewSignals = GetSignalsByPageName(viewName);

            pulseInViewSignals.ForEach(signal =>
            {
                if(signal.SignalName.IndexOf("_Duty") > -1 || signal.SignalName.IndexOf("_Freq") > -1)
                {
                    string[] groupName = signal.SignalName.Split(new string[] { "_Duty", "_Freq" }, StringSplitOptions.RemoveEmptyEntries);
                    if (groupName.Length == 1)
                    {
                        PulseInSignal pulseInSignal = new PulseInSignal(groupName[0])
                        {
                            Name = signal.SignalName,
                            StartBit = (int)signal.startBit,
                            Factor = signal.factor,
                            Offset = signal.offset,
                            ByteOrder = (int)signal.byteOrder,
                            Length = (int)signal.signalSize,
                            MessageID = signal.MessageID,
                            ViewName = viewName
                        };
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
                if(startbit + signalLength * 2 > 64 * 8 -1)
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
        }

        private void GenerateVirtualSignals<TSignal>(ref uint id, string viewName,ref int startbit, int count = 10, int signalLength = 1)
          where TSignal : SignalBase, new()
        {
            
            for (int i = 0; i < count; i++)
            {
                startbit += signalLength * i;
                if(startbit + signalLength > 64 * 8 - 1)
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

        #region Signal Locator
        [Obsolete]
        public ViewsSignals SignalLocatorInfo { get; private set; }
        public SignalCollection SignalLocation { get; private set; }
        private const string SignalLocatorFilePath = @"Config/SignalLocator.xml";
        //private const string SignalLocatorFilePath2 = @"Config/SignalLocator2.xml";
        private void LoadSignalLocator()
        {
            //ViewsSignals x = new ViewsSignals();
            //XmlHelper.SerializeToXml(x, SignalLocatorFilePath);
            SignalLocation = XmlHelper.DeserializeFromXml<SignalCollection>(SignalLocatorFilePath);
            if(SignalLocation == null)
            {
                SignalLocation = new SignalCollection();
            }
            //foreach (var view in SignalLocation.ViewSignalsInfos)
            //{
            foreach (var signal in SignalLocation.Signals)
            {
                AddSignal(signal);
                if (signal is DiscreteOutputSignal disout)
                {
                    AddSignal(disout.State);
                }
            }
            //}
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
                if(SignalLocation == null) { SignalLocation = new SignalCollection(); }

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
        
        public double ConvertByTable(string tableName,double origionalValue)
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
