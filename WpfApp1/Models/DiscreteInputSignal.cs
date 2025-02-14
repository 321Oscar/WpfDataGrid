using System.ComponentModel;
using System.Xml.Serialization;

namespace WpfApp1.Models
{
    public class DiscreteInputSignal : DiscreteSignal
    {
        private int transitions;
        [XmlIgnore]
        public int Transitions { get => transitions; private set => SetProperty(ref transitions , value); }

        public DiscreteInputSignal()
        {
            //this.PropertyChanged += DiscreteSignal_PropertyChanged;
        }

        public void ClearTransitions()
        {
            Transitions = 0;
        }
        public override void OnOriginValueChaned(double originValue, bool equal)
        {
            base.OnOriginValueChaned(originValue, equal);
            if (equal)
            {
                Transitions += 1;
            }
        }
        //public override void on()
        //{
        //    base.OnRealValueChanged();
        //    Transitions += 1;
        //}
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
