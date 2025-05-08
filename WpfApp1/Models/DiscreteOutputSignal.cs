using System.Linq;
using System.Xml.Serialization;

namespace ERad5TestGUI.Models
{
    /// <summary>
    /// OriginValue Only 1/0
    /// </summary>
    public class DiscreteOutputSignal : DiscreteSignal, ISyncValue
    {
        private DiscreteInputSignal state;
        private bool _isOutput = true;

        //public new bool InOrOut { get; }

        public DiscreteOutputSignal()
        {
            //PropertyChanged += DiscreteOutputSignal_PropertyChanged;
            InOrOut = true;
        }

        public DiscreteOutputSignal(Stores.Signal signal, string viewName) : base(signal, viewName) { InOrOut = true; }

        [XmlIgnore]
        public double? TempValue
        {
            get;
            set;
        }
        /// <summary>
        /// 如果是同步的，则把数据存在Temp中
        /// </summary>
        [XmlIgnore]
        public bool Sync { get; set; }
        [XmlIgnore]
        public bool Pin_High
        {
            get
            {
                if (Sync && TempValue.HasValue)
                    return TempValue.Value == 1;
                return OriginValue == 1;
            }
            set
            {
                if (Sync)
                {
                    TempValue = value ? 1 : 0;
                    OnPropertyChanged(nameof(Pin_High));
                    OnPropertyChanged(nameof(Pin_Low));
                }
                else
                {
                    OriginValue = value ? 1 : 0;
                }
            }
        }
        [XmlIgnore]
        public bool Pin_Low
        {
            get
            {
                if (Sync && TempValue.HasValue)
                    return TempValue.Value == 0;
                return OriginValue == 0;
            }
            set
            {
                if (Sync)
                {
                    TempValue = value ? 0 : 1;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Pin_High));
                }
                else
                {
                    OriginValue = value ? 0 : 1;
                }
            }
        }
        public bool IsOutput 
        { 
            get => _isOutput; 
            set => SetProperty(ref _isOutput, value); 
        }

        public override void Clear()
        {
            if (State != null)
            {
                State.OriginValue = double.NaN;
            }
            else
            {
                base.Clear();
            }
        }

        protected override void OnOriginValueChaned(double originValue, bool equal)
        {
            base.OnOriginValueChaned(originValue, equal);
            if (equal)
            {
                OnPropertyChanged(nameof(Pin_High));
                OnPropertyChanged(nameof(Pin_Low));
            }
        }

        public void UpdateRealValue()
        {
            if (TempValue.HasValue)
            {
                OriginValue = TempValue.Value;
                TempValue = null;
            }
        }

        public DiscreteInputSignal State
        {
            get => state;
            set
            {
                if (state != null)
                {
                    state.OnPinChanged -= StateChanged;
                }
                state = value;
                if (state != null)
                {
                    state.OnPinChanged += StateChanged;
                }
            }
        }

        private void StateChanged(double x)
        {
            this.TempValue = null;
            this.OriginValue = x;
        }

        public bool SetStateSignal(Stores.SignalStore signalStore)
        {
            var signalState = signalStore.DBCSignals.FirstOrDefault(x => x.SignalName == Name + "_State");
            if (signalState != null)
            {
                var existInput = signalStore.Signals.FirstOrDefault(x => x.Name == Name + "_State");
                if (existInput != null && existInput is DiscreteInputSignal exsitDiscrete)
                {
                    this.State = exsitDiscrete;
                }
                else
                {
                    var input = new DiscreteInputSignal()
                    {
                        Name = signalState.SignalName,
                        StartBit = (int)signalState.startBit,
                        Factor = signalState.factor,
                        Offset = signalState.offset,
                        ByteOrder = (int)signalState.byteOrder,
                        Length = (int)signalState.signalSize,
                        MessageID = signalState.MessageID,
                    };
                    this.State = input;
                    signalStore.AddSignal(input);
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        public void SetStateSignal(DiscreteInputSignal stateSignal)
        {
            State = stateSignal;
        }

       
    }
}
