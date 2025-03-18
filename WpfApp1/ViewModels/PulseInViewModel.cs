using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using ERad5TestGUI.Models;
using ERad5TestGUI.Services;
using ERad5TestGUI.Stores;
using System.Collections.ObjectModel;

namespace ERad5TestGUI.ViewModels
{
    public class PulseInViewModel : ViewModelBase
    {
        public const string VIEWNAME = "Pulse_IN";
        private readonly ObservableCollection<PulseInSignalGroup> _pulseInGroups = new ObservableCollection<PulseInSignalGroup>();
        private RelayCommand _resetCommand;
        private RelayCommand _updateLimitsCommand;
        private int signalType;

        public PulseInViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService)
                    : base(signalStore, deviceStore, logService)
        {
            this._ViewName = VIEWNAME;
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

        public IEnumerable<PulseInSignalGroup> Groups => _pulseInGroups;

        public ICommand ResetCommand { get => _resetCommand ?? (_resetCommand = new RelayCommand(Reset)); }
        public ICommand UpdateLimitCommand { get => _updateLimitsCommand ?? (_updateLimitsCommand = new RelayCommand(UpdateLimit, () => SignalType > -1)); }

        public override void LocatorSignals()
        {
            if (ModalNavigationStore != null)
            {
                //var modalNavigationService = new ModalNavigationService<PulseInSignalLocatorViewModel>(this.ModalNavigationStore, CreateLocatorViewModel);
                //modalNavigationService.Navigate();
            }
            else
            {
                Views.DialogView dialogView = new Views.DialogView();
                var locatorViewModel = CreateLocatorViewModel(dialogView);
                dialogView.DialogViewModel = locatorViewModel;
                dialogView.ShowDialog();
            }

        }

        private PulseInSignalLocatorViewModel CreateLocatorViewModel(System.Windows.Window window)
         => new PulseInSignalLocatorViewModel(ViewName, _pulseInGroups,
                                          SignalStore,
                                          CreatePulseInGroupSignal, window, AddGroup);

        private PulseInSignalGroup CreatePulseInGroupSignal(Signal signal)
        {
            if (signal.SignalName.IndexOf("_Duty") > -1 || signal.SignalName.IndexOf("_Freq") > -1)
            {
                string[] groupName = signal.SignalName.Split(new string[] { "_Duty", "_Freq" }, StringSplitOptions.RemoveEmptyEntries);
                if (groupName.Length == 1)
                {
                    var group = new PulseInSignalGroup(groupName[0]);
                    var existSignal = SignalStore.Signals.FirstOrDefault(x => x.Name == signal.SignalName && x.MessageID == signal.MessageID);
                    PulseInSignal pulseInSignal;
                    if (existSignal == null || !(existSignal is PulseInSignal analog))
                    {
                        pulseInSignal = new PulseInSignal(signal, ViewName, groupName[0]);
                        SignalStore.AddSignal(pulseInSignal);
                    }
                    else
                    {
                        pulseInSignal = existSignal as PulseInSignal;
                    }
                    if (signal.SignalName.IndexOf("_Duty") > -1)
                    {
                        group.Signal_DC = pulseInSignal;
                    }
                    else
                    {
                        group.Signal_Freq = pulseInSignal;
                    }
                    return group;
                }
            }
            return null;
        }

        private void AddGroup(ObservableCollection<PulseInSignalGroup> groups, PulseInSignalGroup group)
        {
            var existGroup = groups.FirstOrDefault(x => x.GroupName == group.GroupName);
            if (existGroup != null)
            {
                if (group.Signal_Freq != null)
                    existGroup.Signal_Freq = group.Signal_Freq;
                if (group.Signal_DC != null)
                    existGroup.Signal_DC = group.Signal_DC;
            }
            else
            {
                groups.Add(group);
            }
        }

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

            _pulseInGroups.AddRange(SignalStore.GetSignals<PulseInSignal>(VIEWNAME)
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
                                .OrderBy(x => x.GroupName));
                                //.ToList();
        }
    }
}