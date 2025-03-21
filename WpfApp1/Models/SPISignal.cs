using ERad5TestGUI.Stores;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ERad5TestGUI.Models
{
    /// <summary>
    /// Signal Value:Enum
    /// </summary>
    public class SPISignal : SignalBase
    {
        private string value1 = "2";

        public SPISignal()
        {

        }
        [XmlIgnore]
        public string Value1
        {
            get => value1;
            set
            {
                SetProperty(ref value1, value);
            }
        }
        [XmlIgnore]
        public bool IsUsed { get; set; } = true;
        /// <summary>
        /// 特定信号，
        /// </summary>
        [XmlIgnore]
        public bool FixedOut { get; set; }
        public SPISignal(Stores.Signal signal, string viewName = "SPI") : base(signal, viewName)
        {

        }
        public string Note { get; set; } = "Note";
        public string ChannelName
        {
            get
            {
                if (Name.IndexOf("SPI") > -1)
                    return Name.Split("SPI")[0].TrimEnd('_');
                return Name;
            }
        }
        public override void UpdateFormDBC(Signal signal)
        {
            base.UpdateFormDBC(signal);
           
        }
        public override void OnOriginValueChaned(double originValue, bool changed)
        {
            base.OnOriginValueChaned(originValue, changed);
            if (changed)
            {
                Value1 = Value2Description(OriginValue);
            }
        }

        public string Value2Description(double val)
        {
            if(Value2State == null)
            {
                if (SPIValueTable.Value2Baudrate.TryGetValue(OriginValue, out string valStr))
                    return valStr;
            }
            else if (Value2State.TryGetValue(OriginValue, out string valStr))
                return valStr;

            return val.ToString();
        }

        public void UpdateEnum(Dictionary<int, string> keyValuePairs)
        {
            if (Value2State == null)
            {
                Value2State = new Dictionary<double, string>();
            }
            else
                Value2State.Clear();

            foreach (var keyVal in keyValuePairs)
            {
                Value2State.Add(keyVal.Key, keyVal.Value);
            }

            Value1 = Value2Description(OriginValue);
        }

        [XmlIgnore]
        public Dictionary<double, string> Value2State { get; set; }
    }

    public class SPISignalGroup : IGroupSignal
    {
        public string GroupName { get; set; }

        public SPISignalGroup(string groupName)
        {
            GroupName = groupName;
        }
        public void UpdateEnum(Dictionary<int, string> keyValuePairs)
        {
            CurrentValue?.UpdateEnum(keyValuePairs);
            SelectValue?.UpdateEnum(keyValuePairs);
        }
        public SPISignal CurrentValue { get; set; }
        public SPISignal SelectValue { get; set; }

        public string Note
        {
            get
            {
                if (SelectValue != null && SelectValue.Note != "Note")
                {
                    return SelectValue.Note;
                }
                if (CurrentValue != null && CurrentValue.Note != "Note")
                {
                    return CurrentValue.Note;
                }

                return "";
            }
        }
    }
    public static class SPIValueTable
    {
        public static Dictionary<double, string> Value2Baudrate = new Dictionary<double, string>();
    }
}
