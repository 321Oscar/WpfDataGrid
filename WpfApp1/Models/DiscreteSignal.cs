using CommunityToolkit.Mvvm.ComponentModel;
using ERad5TestGUI.Stores;
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
            
        }

        public override void UpdateFormDBC(Signal signal)
        {
            base.UpdateFormDBC(signal);
            PinNumber = signal.Comment.GetCommentByKey("Pin_Number");
        }

        public string PinNumber { get; set; }
    }

}
