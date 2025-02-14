using System.Collections.Generic;
using System.Xml.Serialization;

namespace WpfApp1.Models
{
    public class PulseInSignalGroup : SignalGroupBase
    {
        //public string SignalName { get; set; }

        public PulseInSignalGroup(string signalName) : base(signalName)
        {
            //SignalName = signalName;
        }

        public PulseInSignal Signal_DC { get; set; }

        public PulseInSignal Signal_Freq { get; set; }
    }

    public class PulseInGroupGroup : SignalGroupBase
    {
        public System.Collections.Generic.List<PulseInSignalGroup> Groups { get; }
        public PulseInGroupGroup(string groupName) : base(groupName)
        {
            Groups = new System.Collections.Generic.List<PulseInSignalGroup>();
        }
    }

    public class PulseInSignal : LimitsSignalBase, IGroupSignal
    {
        public string GroupName { get; set; }
        public PulseInSignal()
        {

        }
        public PulseInSignal(string groupName)
        {
            GroupName = groupName;
        }
    }

    public class PulseOutSingleSignal : TransFormSignalBase, ISyncValue
    {
        private double tempValue;
        [XmlIgnore]
        public double? TempValue { get => tempValue; set => SetProperty(ref tempValue, value.Value); }
        [XmlIgnore]
        public bool Sync { get; set; } = true;

        public override void OnOriginValueChaned(double originValue, bool changed)
        {
            if (changed)
                TempValue = TransForm(originValue);
        }

        public void UpdateRealValue()
        {
            OriginValue = tempValue;
        }
    }
    /// <summary>
    /// include PulseOutSingleSignal
    /// </summary>
    public class PulseOutSingleSignalGroup : SignalGroupBase
    {
        public List<PulseOutSingleSignal> Signals { get; }
        public PulseOutSingleSignalGroup(string groupName) : base(groupName)
        {
            Signals = new List<PulseOutSingleSignal>();
        }
    }

    /// <summary>
    /// <see cref="SignalBase.Name"/> Only Frequency Or DutyCycle
    /// <para><see cref="GroupName"/> is the SignalName</para>
    /// </summary>
    public class PulseOutGroupSignal : PulseOutSingleSignal, IGroupSignal
    {
        public string GroupName { get; set; }
        public PulseOutGroupSignal()
        {

        }
        public PulseOutGroupSignal(string groupName)
        {
            GroupName = groupName;
        }

    }
    /// <summary>
    /// included Frequency and Duty Cycle two signals Group
    /// </summary>
    public class PulseGroupSignalOutGroup : SignalGroupBase
    {
        private PulseOutGroupSignal freq;
        private PulseOutGroupSignal dutyCycle;

        public PulseGroupSignalOutGroup(string groupName) : base(groupName)
        {
        }

        public PulseOutGroupSignal Freq { get => freq; set =>SetProperty(ref freq ,value); }
        public PulseOutGroupSignal DutyCycle { get => dutyCycle; set => SetProperty(ref dutyCycle, value); }
    }
}
