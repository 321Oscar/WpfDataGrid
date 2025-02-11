using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using WpfApp1.Models;
using WpfApp1.Services;
using WpfApp1.Stores;

namespace WpfApp1.ViewModels
{
    public class PulseOutViewModel : SendFrameViewModelBase
    {
        private IEnumerable<PulseGroupSignalOutGroup> groups;
        private RelayCommand updateCommand;

        public PulseOutViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService) 
            : base(signalStore, deviceStore, logService)
        {
            BuildFramesHelper = new DBCSignalBuildHelper(PulseOutSignals, signalStore.DbcFile.Messages);
            updateCommand = new RelayCommand(Update);
            GetGroups();
        }

      
        public ICommand UpdateCommand { get => updateCommand; }
        public IEnumerable<PulseGroupSignalOutGroup> Groups => groups;
        public IEnumerable<PulseOutSingleSignal> PulseOutSignals => SignalStore.GetSignals<PulseOutSingleSignal>();
        private void GetGroups()
        {
            var gdicSignals = SignalStore.GetSignals<PulseOutGroupSignal>();

            groups = gdicSignals
            .GroupBy(s => s.GroupName)
            .Select(g =>
            {
                if (!string.IsNullOrEmpty(g.Key))
                {
                    var group = new PulseGroupSignalOutGroup(g.Key);
                    var signals = g.ToList();
                    signals.Sort((x, y) =>
                    {
                        return x.Name.CompareTo(y.Name);
                    });
                    group.Freq = signals[0];
                    group.DutyCycle = signals[1];
                    return group;
                }
                return null;
            })
            .OrderBy(x=>x.GroupName)
            .ToList();
            //gDICStatusGroups.Sort
        }

        private void Update()
        {
            foreach (var group in Groups)
            {
                group.Freq.UpdateRealValue();
                group.DutyCycle.UpdateRealValue();
            }

            Send();
        }
    }
}
