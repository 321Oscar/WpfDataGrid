using CommunityToolkit.Mvvm.ComponentModel;
using System.Xml.Serialization;

namespace ERad5TestGUI.Models
{
    public class DiscreteSignal : SignalBase
    {
        public DiscreteSignal()
        {

        }

        public DiscreteSignal(Stores.Signal signal, string viewName) : base(signal, viewName)
        {
            PinNumber = signal.Comment.GetCommentByKey("Pin_Number");
        }

        public string PinNumber { get; set; }
    }

}
