using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.Models;
using WpfApp1.Services;
using WpfApp1.Stores;

namespace WpfApp1.ViewModels
{
    public class PulseOutViewModel : ViewModelBase
    {
        public PulseOutViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService) 
            : base(signalStore, deviceStore, logService)
        {
            GetGroups();
        }

        private IEnumerable<PulseOutGroup> groups;

        public IEnumerable<PulseOutGroup> Groups => groups;
        private void GetGroups()
        {
            var gdicSignals = SignalStore.GetSignals<PulseOutSignal>();

            groups = gdicSignals
            .GroupBy(s => s.GroupName)
            .Select(g =>
            {
                var classRoom = new PulseOutGroup(g.Key);
                var signals = g.ToList();
                signals.Sort((x, y) =>
                {
                    return x.Name.CompareTo(y.Name);
                });
                classRoom.Freq = signals[0];
                classRoom.DutyCycle = signals[1];
                return classRoom;
            })
            .OrderBy(x => x.SignalName)
            .ToList();
            //gDICStatusGroups.Sort
        }
    }
}
