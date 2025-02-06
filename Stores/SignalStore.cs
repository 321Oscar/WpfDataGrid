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
            LoadAnalogSignals();
            LoadDiscretes();
            LoadSavingLogicSignals();
        }

        private void LoadAnalogSignals()
        {
            foreach (var message in DbcFile.Messages.FindAll(x => x.MessageID == 0x6f8))
            {
                foreach (var signal in message.signals)
                {
                    AnalogSignal analogSignal = new AnalogSignal()
                    {
                        Name = signal.signalName,
                        StartBit = (int)signal.startBit,
                        Factor = signal.factor,
                        Offset = signal.offset,
                        ByteOrder = (int)signal.byteOrder,
                        Length = (int)signal.signalSize,
                        MessageID = message.MessageID
                    };

                    Comment comment = DbcFile.Comments.Find(x => x.signalName == signal.signalName
                       && x.messageID == message.MessageID.ToString());
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
                
            }
            //for (int i = 0; i < 10; i++)
            //{
            //    _signals.Add(new AnalogSignal()
            //    {
            //        Name = $"AnalogSignal{i}",
            //        RealValue = i
            //    });
            //}
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

        private void LoadSavingLogicSignals()
        {
            for (int i = 0; i < 10; i++)
            {
                _signals.Add(new SavingLogicSignal()
                {
                    Name = $"SavingLogicSignalInput{i}",
                    Group = "level 1",
                    InputOrOutput = true,
                    MessageID = 0x102,
                    StartBit = i,
                    Length = 1,
                    Factor = 1,
                    Offset = 0,
                    ByteOrder = 1
                });
            }
        }

        public IEnumerable<SignalBase> Signals => _signals;

        public IEnumerable<TSignal> GetSignals<TSignal>() where TSignal : SignalBase
        {
            return _signals.OfType<TSignal>();
        }

        public ObservableCollection<TSignal> GetObservableCollection<TSignal>() where TSignal : SignalBase
        {
            ObservableCollection<TSignal> signals = new ObservableCollection<TSignal>();

            foreach (var item in _signals.OfType<TSignal>())
            {
                signals.Add(item);
            }

            return signals;
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
    }
}
