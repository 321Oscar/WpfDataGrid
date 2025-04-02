using System;
using System.ComponentModel;
using System.Xml.Serialization;
using ERad5TestGUI.Stores;

namespace ERad5TestGUI.Models
{
    public class DiscreteInputSignal : DiscreteSignal
    {
        private int transitions;
        [XmlIgnore]
        public int Transitions { get => transitions; private set => SetProperty(ref transitions , value); }
        [XmlIgnore]
        public Action<double> OnPinChanged { get; set; }
        public DiscreteInputSignal()
        {
            //this.PropertyChanged += DiscreteSignal_PropertyChanged;
        }

        public DiscreteInputSignal(Signal signal, string viewName) : base(signal, viewName)
        {

        }
        public override void Clear()
        {
            base.Clear();
            ClearTransitions();
        }
        public void ClearTransitions()
        {
            Transitions = 0;
        }
        protected override void OnOriginValueChaned(double originValue, bool changed)
        {
            base.OnOriginValueChaned(originValue, changed);
            if (changed)
            {
                if (NeedTransitions)
                    Transitions += 1;
                OnPinChanged?.Invoke(originValue);
            }
        }

        public bool NeedTransitions { get; set; } = true;
        //public override void on()
        //{
        //    base.OnRealValueChanged();
        //    Transitions += 1;
        //}
        public bool IsEnable { get => MessageID != 0; }
    }
    public class DiscreteInputSignalGroup : SignalGroup<DiscreteInputSignal>
    {
        public DiscreteInputSignalGroup(string groupName) : base(groupName)
        {
        }
    }

    public class DiscreteOutputSignalGroup : SignalGroup<DiscreteOutputSignal>
    {
        public DiscreteOutputSignalGroup(string groupName) : base(groupName)
        {
        }
    }
}
