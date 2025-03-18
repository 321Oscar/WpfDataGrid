using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using ERad5TestGUI.Models;
using ERad5TestGUI.Services;
using ERad5TestGUI.Stores;

namespace ERad5TestGUI.ViewModels
{
    public class ResolverViewModel : ViewModelBase
    {
        private readonly ObservableCollection<AnalogSignal> _analogSignals = new ObservableCollection<AnalogSignal>();
        private readonly ObservableCollection<ResolverSignal> _resolverSignals = new ObservableCollection<ResolverSignal>();
        private readonly ObservableCollection<PulseInSignalGroup> _pulseInGroups = new ObservableCollection<PulseInSignalGroup>();
        private readonly ObservableCollection<DiscreteInputSignal> _discreteInputSignals = new ObservableCollection<DiscreteInputSignal>();
 
        private RelayCommand _locatorAverageSignalsCommand;

        public ResolverViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService) : base(signalStore, deviceStore, logService)
        {
            
        }

        public ResolverViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService, ModalNavigationStore modalNavigationStore, IServiceProvider serviceProvider) : base(signalStore, deviceStore, logService, modalNavigationStore, serviceProvider)
        {
        }
        public IEnumerable<PulseInSignalGroup> PulseInGroups { get => _pulseInGroups; }
        public IEnumerable<DiscreteInputSignal> DiscreteInputs { get => _discreteInputSignals; }
        public IEnumerable<ResolverSignal> ResolverSignals { get => _resolverSignals; }
        public IEnumerable<AnalogSignal> AnalogSignals
        {
            get
            {
                return _analogSignals;
            }
        }

        public ICommand LocatorAverageSignalsCommand { get => _locatorAverageSignalsCommand ?? (_locatorAverageSignalsCommand = new RelayCommand(LocatorAverageSignals)); }

        public override void Init()
        {
            base.Init();
            var analogs = SignalStore.GetSignals<AnalogSignal>(ViewName);

            _analogSignals.AddRange(analogs);

            var groups = SignalStore.GetSignals<PulseInSignal>(ViewName)
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
                               .OrderBy(x => x.GroupName);
            _pulseInGroups.AddRange(groups);

            _discreteInputSignals.AddRange(SignalStore.GetSignals<DiscreteInputSignal>(ViewName));
            _resolverSignals.AddRange(SignalStore.GetSignals<ResolverSignal>(ViewName));
        }

        public override void LocatorSignals()
        {
            if (ModalNavigationStore != null)
            {
               // var modalNavigationService = new ModalNavigationService<PulseOutSignalLocatorViewModel>(this.ModalNavigationStore, CreateLocatorViewModel);
               // modalNavigationService.Navigate();
            }
            else
            {
                Views.DialogView dialogView = new Views.DialogView();
                var locatorViewModel = CreateLocatorViewModel(dialogView);
                dialogView.DialogViewModel = locatorViewModel;
                dialogView.ShowDialog();
            }
        }

        private void LocatorAverageSignals()
        {
            if (ModalNavigationStore != null)
            {
                // var modalNavigationService = new ModalNavigationService<PulseOutSignalLocatorViewModel>(this.ModalNavigationStore, CreateLocatorViewModel);
                // modalNavigationService.Navigate();
            }
            else
            {
                Views.DialogView dialogView = new Views.DialogView();
                var locatorViewModel = CreateSignalLocatorViewModel(dialogView);
                dialogView.DialogViewModel = locatorViewModel;
                dialogView.ShowDialog();
            }
        }

        private ResolverSignalLocatorViewModel CreateSignalLocatorViewModel(System.Windows.Window window)
           => new ResolverSignalLocatorViewModel(ViewName, _resolverSignals,
                                               SignalStore,
                                               CreateAnalogByDBCSignal,
                                               window);

        private ResolverSignal CreateAnalogByDBCSignal(Signal signal)
        {
            var existSignal = SignalStore.Signals.FirstOrDefault(x => x.Name == signal.SignalName && x.MessageID == signal.MessageID);
            if (existSignal != null && existSignal is ResolverSignal analog)
                return analog;

            ResolverSignal analogSignal = new ResolverSignal(signal, ViewName)
            {
                NeedTransform = false,
            };
            SignalStore.AddSignal(analogSignal);
            return analogSignal;
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
    }
}
