using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WpfApp1.Devices;
using WpfApp1.Models;
using WpfApp1.Services;

namespace WpfApp1.Stores
{
    public class SignalStore
    {
        private readonly List<SignalBase> _signals;
        private readonly LogService logService;
        //public const double AnalogConst = 5 / 4096;
        public DbcFile DbcFile { get; }
        public SignalStore(Services.LogService logService)
        {
            this.logService = logService;
            _signals = new List<SignalBase>();
            DbcFile = new DbcFile(@".\Config\Erad5_GUI_DEVCAN.dbc");
            LoadAnalogSignals(0x6f8, nameof(ViewModels.AnalogViewModel));
            LoadAnalogSignals(0x605, nameof(ViewModels.ResolverViewModel));
            LoadPPAWL(0x01, nameof(ViewModels.PPAWLViewModel));
            LoadDiscretes();
            LoadSavingLogicSignals();
            LoadGDICStatusSignals();
            LoadPulseInSignals(0x10, nameof(ViewModels.PulseInViewModel));
            LoadPulseInSignals(0x20, nameof(ViewModels.PPAWLViewModel));
            LoadPulseOutSignals(nameof(ViewModels.PPAWLViewModel));
            LoadPulseOutSignals(nameof(ViewModels.PulseOutViewModel));
        }
        public IEnumerable<SignalBase> Signals => _signals;

        public IEnumerable<TSignal> GetSignals<TSignal>(string viewName = "") where TSignal : SignalBase
        {
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
                               return x.ViewName == viewName;
                           });
        }

        public ObservableCollection<TSignal> GetObservableCollection<TSignal>(string viewName = "") where TSignal : SignalBase
        {
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
                else if (item.ViewName == viewName)
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

                    if ((msg[byteIndex] & (1 << bitIndex)) != 0)
                    {
                        rawValue |= (1 << i);
                    }
                }
            }

            double physicalValue = rawValue * signal.Factor + signal.Offset;

            return physicalValue;

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

        private void LoadAnalogSignals(uint id, string viewName)
        {
            var message6f8 = DbcFile.Messages.FindAll(x => x.MessageID == id).FirstOrDefault();

            foreach (var signal in message6f8.signals)
            {
                AnalogSignal analogSignal = new AnalogSignal()
                {
                    Name = signal.signalName,
                    StartBit = (int)signal.startBit,
                    Factor = signal.factor,
                    Offset = signal.offset,
                    ByteOrder = (int)signal.byteOrder,
                    Length = (int)signal.signalSize,
                    MessageID = message6f8.MessageID,
                    ViewName = viewName
                };

                Comment comment = DbcFile.Comments.Find(x => x.signalName == signal.signalName
                   && x.messageID == message6f8.MessageID.ToString());
                if (comment != null)
                {
                    string commentStr = comment.comment;
                    string[] strs = commentStr.Split(new string[] { " ", ":" }, StringSplitOptions.RemoveEmptyEntries);
                    if (strs.Length < 4)
                        continue;
                    analogSignal.PinNumber = strs[1];
                    analogSignal.ADChannel = strs[3];
                }
                _signals.Add(analogSignal);
            }
            logService.Info("add Analog Signals");
        }

        private void LoadDiscretes()
        {
            //input
            for (int i = 0; i < 10; i++)
            {
                _signals.Add(new DiscreteInputSignal()
                {
                    Name = $"DiscreteSignalInput{i}",
                    MessageID = 0x101,
                    StartBit = i,
                    Length = 1,
                    Factor = 1,
                    Offset = 0,
                    ByteOrder = 1
                });
            }
            for (int i = 0; i < 10; i++)
            {
                _signals.Add(new DiscreteOutputSignal()
                {
                    Name = $"DiscreteSignalOutput{i}",
                    MessageID = 0x100,
                    StartBit = i,
                    Length = 1,
                    Factor = 1,
                    Offset = 0,
                    ByteOrder = 1,
                });
            }
        }

        private void LoadPPAWL(uint msgid,string viewName)
        {
            int startbit = 0;
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
            //Top-U
            GenerateGDICSignals("Top-U");
            GenerateGDICSignals("Top-V");
            GenerateGDICSignals("Top-W");
            GenerateGDICSignals("Bot-U");
            GenerateGDICSignals("Bot-V");
            GenerateGDICSignals("Bot-W");
        }

        private void GenerateGDICSignals(string groupName)
        {
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    GDICStatusSignal gDICStatusSignal = new GDICStatusSignal($"{groupName} Status-{i + 1}")
                    {
                        Name = $"Data{j}"
                    };
                    _signals.Add(gDICStatusSignal);
                }
            }
        }

        private void LoadPulseInSignals(uint msgID, string viewName)
        {
            int startbit = 0;
            GeneratePulseInSignals(viewName, 12, ref msgID, ref startbit);
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
            for (int i = 0; i < 4; i++)
            {
                PulseOutGroupSignal signal_dc = new PulseOutGroupSignal($"PulseOutSignal {i}")
                {
                    Name = "Feq",
                    ViewName = viewName
                };
                PulseOutGroupSignal signal_Freq = new PulseOutGroupSignal($"PulseOutSignal {i}")
                {
                    Name = "Duty",
                    ViewName = viewName
                };
                _signals.Add(signal_dc);
                _signals.Add(signal_Freq);
            }

            _signals.Add(new PulseOutSingleSignal()
            {
                Name = "Freqency",
                ViewName = viewName
            }); _signals.Add(new PulseOutSingleSignal()
            {
                Name = "PWMMode",
                ViewName = viewName
            });
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
    }
}
