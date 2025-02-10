using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace WpfApp1.Models
{
    public class GDICStatusGroup : ObservableObject
    {
        public string GroupName { get; set; }

        public GDICStatusGroup(string groupName)
        {
            GroupName = groupName;
            GDICStatusSignals = new List<GDICStatusSignal>();
        }

        public List<GDICStatusSignal> GDICStatusSignals { get; set; }
    }

    public class GDICStatusSignal : TransFormSignalBase
    {
        public GDICStatusSignal(string groupName)
        {
            GroupName = groupName;
        }

        public string GroupName { get; }
    }

}
