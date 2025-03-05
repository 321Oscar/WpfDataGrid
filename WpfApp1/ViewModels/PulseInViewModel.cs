using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using ERad5TestGUI.Models;
using ERad5TestGUI.Services;
using ERad5TestGUI.Stores;

namespace ERad5TestGUI.ViewModels
{
    public class PulseInViewModel : ViewModelBase
    {
        public const string VIEWNAME = "Pulse_IN";
        private IEnumerable<PulseInSignalGroup> groups;
        private RelayCommand _resetCommand;
        private RelayCommand _updateLimitsCommand;
        private int signalType;

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

        public int SignalType { get => signalType; set { signalType = value; _updateLimitsCommand.NotifyCanExecuteChanged(); } }
        public int MaxThreshold { get; set; }
        public int MinThreshold { get; set; }

        public IEnumerable<PulseInSignalGroup> Groups => groups;

        public ICommand ResetCommand { get => _resetCommand ?? (_resetCommand = new RelayCommand(Reset)); }
        public ICommand UpdateLimitCommand { get => _updateLimitsCommand ?? (_updateLimitsCommand = new RelayCommand(UpdateLimit, () => SignalType > -1)); }

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

        private void UpdateLimit()
        {
            if (SignalType == 0)//duty
            {
                foreach (var group in Groups)
                {
                    group.Signal_DC.MaxThreshold = MaxThreshold;
                    group.Signal_DC.MinThreshold = MinThreshold;
                }
            }
            else if (SignalType == 1)
            {
                foreach (var group in Groups)
                {
                    group.Signal_Freq.MaxThreshold = MaxThreshold;
                    group.Signal_Freq.MinThreshold = MinThreshold;
                }
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
                                    group.Signal_DC = signals.FirstOrDefault(x => x.Name.IndexOf("DC") > -1 || x.Name.IndexOf("Duty") > -1);
                                    group.Signal_Freq = signals.FirstOrDefault(x => x.Name.IndexOf("Freq") > -1);
                                    return group;
                                })
                                .OrderBy(x => x.GroupName)
                                .ToList();
        }
    }
}