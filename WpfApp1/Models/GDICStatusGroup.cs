using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using ERad5TestGUI.Stores;

namespace ERad5TestGUI.Models
{
    public class GDICStatusRegisterGroup : ObservableObject
    {
        /// <summary>
        /// RegisterName like: Top U/Top V
        /// </summary>
        public string GroupName { get; }

        public GDICStatusRegisterGroup(string groupName)
        {
            GroupName = groupName;
            GDICStatusGroups = new GDICStatusGroup[4];
        }
        /// <summary>
        /// Array of Status that contains 10 Data
        /// </summary>
        public GDICStatusGroup[] GDICStatusGroups { get; }
        /// <summary>
        /// 1 Group 
        /// </summary>
        public GDICStatusGroup WriteStatus { get; set; }

        public override string ToString()
        {
            return GroupName;
        }
    }

    /// <summary>
    /// it's Name like Status1/Status2
    /// </summary>
    public class GDICStatusGroup : ObservableObject
    {
        private string groupName;

        /// <summary>
        /// RegisterName like: Top U/Top V
        /// </summary>
        public string GroupName { get => groupName; set => groupName = value.Replace(SignalBase.DBCSignalNameSplit, SignalBase.SignalNameSplit); }

        public GDICStatusGroup(string groupName)
        {
            GroupName = groupName;
            GDICStatusSignals = new GDICStatusDataSignal[10];
        }
        public int Startbit
        {
            get
            {
                var signal = GDICStatusSignals.FirstOrDefault(x => x != null);
                if (signal != null)
                    return signal.StartBit;
                return 0;
            }
        }
        /// <summary>
        /// In : false
        /// <para>Out: true</para>
        /// </summary>
        public bool InOrOut
        {
            get
            {
                var signal = GDICStatusSignals.FirstOrDefault(x => x != null);
                if (signal != null)
                    return signal.InOrOut;
                return false;
            }
        }
        /// <summary>
        /// Write or Read Data [9-10]
        /// </summary>
        public GDICStatusDataSignal[] GDICStatusSignals { get; }
        /// <summary>
        /// if <see cref="InOrOut"/> == true, need this property
        /// </summary>
        public GDICStatusDataSignal WriteIndex { get; set; }
        public string RegisterName
        {
            get
            {
                if (!string.IsNullOrEmpty(GroupName))
                {
                    return string.Join(SignalBase.SignalNameSplit, GroupName.Split(SignalBase.SignalNameSplit).Take(2));
                }
                return "";
            }
        }

        public string DisplayName { get => GroupName.Replace(RegisterName, "").Replace("_", ""); }

        public override string ToString()
        {
            return GroupName;
        }
    }
    /// <summary>
    /// Input and Output 
    /// </summary>
    public class GDICStatusDataSignal : TransFormSignalBase
    {
        public GDICStatusDataSignal()
        {

        }

        public GDICStatusDataSignal(string groupName)
        {
            GroupName = groupName;
        }

        public GDICStatusDataSignal(Stores.Signal signal, string viewName) : base(signal, viewName)
        {
            string strs = string.Join("_", Name.Split("_").Take(3));
            this.GroupName = strs;
        }
        /// <summary>
        /// like:TOP_U
        /// </summary>
        public string RegisterName { get => string.Join("_", Name.Split("_").Take(2)); }

        /// <summary>
        /// Status Name like Status1
        /// </summary>
        public string GroupName { get; set; }

        //public new string DisplayName { get => Name.Replace(GroupName, "").Replace("_", "").Replace("Write", ""); }

        public override string RelaceSignalName(string s)
        {
            var name = base.RelaceSignalName(Name);
            return name.Replace(GroupName, "").Replace("_", "").Replace("Write", "");
        }
    }

}
