using System.Collections.Generic;

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
        public string GroupName { get; }

        public PulseInSignal(string groupName)
        {
            GroupName = groupName;
        }
    }

    public class PulseOutSingleSignal : TransFormSignalBase, ISyncValue
    {
        private double tempValue;

        public double? TempValue { get => tempValue; set => SetProperty(ref tempValue, value.Value); }
        public bool Sync { get; set; } = true;

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
        public string GroupName { get; }

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
        public PulseGroupSignalOutGroup(string groupName) : base(groupName)
        {
        }

        public PulseOutGroupSignal Freq { get; set; }
        public PulseOutGroupSignal DutyCycle { get; set; }
    }
}
