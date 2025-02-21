using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using WpfApp1.Models;
using WpfApp1.Services;
using WpfApp1.Stores;

namespace WpfApp1.ViewModels
{
    public class PulseInViewModel : ViewModelBase
    {
        public const string VIEWNAME = "Pulse_IN";
        private IEnumerable<PulseInSignalGroup> groups;
        private RelayCommand _resetCommand;
        public PulseInViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService)
            : base(signalStore, deviceStore, logService)
        {
            GetGroups();
        }

        public PulseInViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService, ModalNavigationStore modalNavigationStore, IServiceProvider serviceProvider) 
            : base(signalStore, deviceStore, logService, modalNavigationStore, serviceProvider)
        {
            GetGroups();
        }

     

        public IEnumerable<PulseInSignalGroup> Groups => groups;

        public ICommand ResetCommand { get => _resetCommand ?? (_resetCommand = new RelayCommand(Reset)); }

        private void Reset()
        {
            foreach (var group in Groups)
            {
                group.Signal_DC.MaxValue = 0;
                group.Signal_DC.MinValue = 0;
                group.Signal_Freq.MaxValue = 0;
                group.Signal_Freq.MinValue = 0;
            }
        }

        private void GetGroups()
        {
            //var gdicSignals = SignalStore.GetSignals<PulseInSignal>(nameof(PulseInViewModel));

            groups = SignalStore.GetSignals<PulseInSignal>(VIEWNAME)
                                .GroupBy(s => s.GroupName)
                                .Select(g =>
                                {
                                    var group = new PulseInSignalGroup(g.Key);
                                    var signals = g.ToList();
                                    signals.Sort((x, y) =>
                                    {
                                        return x.Name.CompareTo(y.Name);
                                    });
                                    group.Signal_DC = signals.FirstOrDefault(x=>x.Name.IndexOf("DC") > -1 || x.Name.IndexOf("Duty") > -1);
                                    group.Signal_Freq = signals.FirstOrDefault(x => x.Name.IndexOf("Freq") > -1);
                                    return group;
                                })
                                .OrderBy(x => x.GroupName)
                                .ToList();
        }
    }
}