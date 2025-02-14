using CommunityToolkit.Mvvm.ComponentModel;
using System.Xml.Serialization;

namespace WpfApp1.Models
{
    public class DiscreteSignal : SignalBase
    {
        [ObservableProperty]
        public string PinNumber { get; set; }
        [XmlIgnore]
        public int Status { get; set; }
    }

}
