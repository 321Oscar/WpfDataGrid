using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace ERad5TestGUI.Models
{
    public class SavingLogicInOutGroup : SignalGroupBase
    {
        public SavingLogicInOutGroup(string signalName) 
            : base(signalName)
        {
            InOutGroups = new List<SavingLogicGroup>();
        }

        public List<SavingLogicGroup> InOutGroups { get; }
    }


    public class SavingLogicGroup : SignalGroupBase
    {
        public SavingLogicGroup(string signalName)
            : base(signalName)
        {
            Signals = new List<SavingLogicSignal>();
        }

        public List<SavingLogicSignal> Signals { get; }
    }

    public class SavingLogicSignal : SignalBase
    {
        public string GroupName { get; set; }
        /// <summary>
        /// true:Input
        /// </summary>
        public bool InputOrOutput { get; set; }

        public bool PinHigh
        {
            get
            {
                return OriginValue == 1;
            }
            set
            {
                OriginValue = value ? 1 : 0;
            }
        }

        public string PinStatus
        {
            get
            {
                return PinHigh ? "High" : "Low";
            }
        }

        protected override void OnOriginValueChaned(double originValue, bool equal)
        {
            base.OnOriginValueChaned(originValue, equal);
            if (equal)
            {
                //base.OnRealValueChanged();
                OnPropertyChanged(nameof(PinHigh));
                OnPropertyChanged(nameof(PinStatus));
            }

        }
    }

    public class SavingLogicButtonSignalGroup : SignalGroupBase
    {
        public SavingLogicButtonSignalGroup(string signalName) : base(signalName)
        {
            Signals = new List<SavingLogicButtonSignal>();
        }

        public List<SavingLogicButtonSignal> Signals { get; }
    }

    public class SavingLogicButtonSignal : TransFormSignalBase
    {
        //private string direction;
        private bool inputOrOut;

        public string Direction { get => OriginValue == 1 ? "Input" : "Output"; }
        
        public bool InputOrOut 
        {
            get => inputOrOut;
            set
            {
                if (SetProperty(ref inputOrOut, value))
                {
                    OriginValue = inputOrOut ? 1 : 0;
                }
            }
        }

        protected override void OnOriginValueChaned(double originValue, bool changed)
        {
            base.OnOriginValueChaned(originValue, changed);
            if (changed)
            {
                //InputOrOut = originValue == 1;
                OnPropertyChanged(nameof(Direction));
            }
        }
    }

    public class SafingLogicDirectionSelect
    {
        private SafingLogicDirectionSignal _currentDirection;

        public SafingLogicDirectionSignal CurrentDirection 
        { 
            get => _currentDirection;
            set
            {
                if (_currentDirection != null)
                    _currentDirection.PropertyChanged -= CurrentDirection_PropertyChanged;
                _currentDirection = value;
                if (_currentDirection != null)
                    _currentDirection.PropertyChanged += CurrentDirection_PropertyChanged;
            }
        }

        private void CurrentDirection_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SignalBase.OriginValue))
            {
                if (Select != null)
                {
                    var cur = sender as SafingLogicDirectionSignal;
                    Select.OriginValue = cur.OriginValue;
                }
            }
        }

        public SafingLogicDirectionSignal Select { get; set; }
    }

    public class SafingLogicDirectionSignal : TransFormSignalBase
    {
        private bool _direction;

        public bool Direction { get => OriginValue == 1; set => OriginValue = value ? 1 : 0; }
        //public string Direction { get; set; }
        private const string Header = "Safing_Logic_";
        private const string End = "_Dir";

        protected override void OnOriginValueChaned(double originValue, bool changed)
        {
            base.OnOriginValueChaned(originValue, changed);
            OnPropertyChanged(nameof(Direction));
        }


        public override string RelaceSignalName(string signalName)
        {
            string baseName =  base.RelaceSignalName(signalName);

            int idxStart = baseName.IndexOf(Header);
            if (idxStart == -1)
                idxStart = 0;
            else
                idxStart += Header.Length;
            int idxEnd = baseName.IndexOf(End) + End.Length;
            int nameLength = idxEnd - idxStart;
            baseName = baseName.Substring(idxStart, nameLength);

            return baseName;
        }
    }

    /**
     * 信号Direction：
     * 1.显示信号当前Dir
     * 2.改变信号Dir，并且修改对应Pin信号的显示
     * 
     * Pin 信号值
     * 1.分为Input/OutPut
     * Input 显示 Pin Low/High
     * Output 发送？
     */

}
