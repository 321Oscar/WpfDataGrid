using System.ComponentModel;

namespace WpfApp1.Models
{
    public class DiscreteInputSignal : DiscreteSignal
    {
        private int transitions;

        public int Transitions { get => transitions; private set => SetProperty(ref transitions , value); }

        public DiscreteInputSignal()
        {
            //this.PropertyChanged += DiscreteSignal_PropertyChanged;
        }
        private void DiscreteSignal_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RealValue))
            {
                Transitions += 1;
            }
        }
        public void ClearTransitions()
        {
            Transitions = 0;
        }

        public override void OnRealValueChanged()
        {
            base.OnRealValueChanged();
            Transitions += 1;
        }
    }

}
