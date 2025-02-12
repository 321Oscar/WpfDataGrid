using CommunityToolkit.Mvvm.ComponentModel;

namespace WpfApp1.Models
{
    public class DiscreteSignal : SignalBase
    {
        [ObservableProperty]
        public string PinNumber { get; set; }
        public int Status { get; set; }
    }

}
