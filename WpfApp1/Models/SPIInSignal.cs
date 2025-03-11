using ERad5TestGUI.Stores;
using System.Collections.Generic;

namespace ERad5TestGUI.Models
{
    public class SPISignal : SignalBase
    {
        private string value1 = "2";

        public SPISignal()
        {

        }

        public string Value1
        {
            get => value1;
            set
            {
                SetProperty(ref value1, value);
            }
        }

        public bool IsUsed { get; set; } = true;
        /// <summary>
        /// 特定信号，
        /// </summary>
        public bool FixedOut { get; set; }
        public SPISignal(Stores.Signal signal, string viewName = "SPI") : base(signal, viewName)
        {

        }

        public override void OnOriginValueChaned(double originValue, bool changed)
        {
            base.OnOriginValueChaned(originValue, changed);
            if (changed && SPIValueTable.Value2Baudrate.TryGetValue(OriginValue, out string val))
            {
                Value1 = val;
            }
        }

        public string ChannelName
        {
            get
            {
                if (Name.IndexOf("SPI") > -1)
                    return Name.Split("SPI")[0].TrimEnd('_');
                return Name;
            }
        }
    }

    public class SPISignalGroup : IGroupSignal
    {
        public string GroupName { get; set; }

        public SPISignalGroup(string groupName)
        {
            GroupName = groupName;
        }

        public SPISignal CurrentValue { get; set; }
        public SPISignal SelectValue { get; set; }
    }
    public static class SPIValueTable
    {
        public static Dictionary<double, string> Value2Baudrate = new Dictionary<double, string>();
    }
}
