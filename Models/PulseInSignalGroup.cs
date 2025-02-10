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
    public class PulseInSignal : LimitsSignalBase
    {
        public string GroupName { get; }

        public PulseInSignal(string groupName)
        {
            GroupName = groupName;
        }
    }
    public class PulseOutSignal : TransFormSignalBase
    {
        public string GroupName { get; }

        public PulseOutSignal(string groupName)
        {
            GroupName = groupName;
        }
    }
    public class PulseOutGroup : SignalGroupBase
    {
        public PulseOutGroup(string groupName) : base(groupName)
        {
        }

        public PulseOutSignal Freq { get; set; }
        public PulseOutSignal DutyCycle { get; set; }
    }
}
