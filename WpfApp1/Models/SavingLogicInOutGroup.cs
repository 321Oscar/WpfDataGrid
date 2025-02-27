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

        public override void OnOriginValueChaned(double originValue, bool equal)
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

        public override void OnOriginValueChaned(double originValue, bool changed)
        {
            base.OnOriginValueChaned(originValue, changed);
            if (changed)
            {
                //InputOrOut = originValue == 1;
                OnPropertyChanged(nameof(Direction));
            }
        }
    }

}
