using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ERad5TestGUI.Models;
using ERad5TestGUI.Services;
using ERad5TestGUI.Stores;

namespace ERad5TestGUI.ViewModels
{
    public class PulseOutViewModel : SendFrameViewModelBase
    {
        public const string VIEWNAME = "Pulse_OUT";

        private readonly ObservableCollection<PulseOutGroupSignalGroup> _groups = new ObservableCollection<PulseOutGroupSignalGroup>();
        private RelayCommand _updateCommand;

        public PulseOutViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService)
            : base(signalStore, deviceStore, logService)
        {
            _ViewName = VIEWNAME;
        }

        public PulseOutViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService, ModalNavigationStore modalNavigationStore, IServiceProvider serviceProvider) 
            : base(signalStore, deviceStore, logService, modalNavigationStore, serviceProvider)
        {
           
        }
        ~PulseOutViewModel()
        {
            Dispose();
        }
      
        public ICommand UpdateCommand { get => _updateCommand ?? (_updateCommand = new RelayCommand(Update)); }
        public IEnumerable<PulseOutGroupSignalGroup> Groups => _groups;
        [Obsolete]
        public IEnumerable<PulseOutSingleSignal> PulseOutSignals => SignalStore.GetSignals<PulseOutSingleSignal>();

        public override void Init()
        {
            GetGroups(SignalStore.GetSignals<PulseOutGroupSignal>(VIEWNAME));
        }

        public override void LocatorSignals()
        {
            if (ModalNavigationStore != null)
            {
                var modalNavigationService = new ModalNavigationService<PulseOutSignalLocatorViewModel>(this.ModalNavigationStore, CreateLocatorViewModel);
                modalNavigationService.Navigate();
            }
            else
            {
                Views.DialogView dialogView = new Views.DialogView();
                var locatorViewModel = CreateLocatorViewModel(dialogView);
                dialogView.DialogViewModel = locatorViewModel;
                dialogView.ShowDialog();
            }

        }
        public override void Send()
        {
            //base.Send();
            if (DeviceStore.HasDevice)
                DeviceStore.CurrentDevice.SendFDMultip(SignalStore.BuildFrames(SignalStore.GetSignals<PulseOutSingleSignal>(VIEWNAME)));
        }
        public override void Dispose()
        {
            //SignalStore.SaveViewSignalLocator(VIEWNAME, PulseOutSignals);
            //SignalStore.SaveViewSignalLocator(VIEWNAME, SignalStore.GetSignals<PulseOutGroupSignal>());
        }

        #region Locator Signals

        private PulseOutSignalLocatorViewModel CreateLocatorViewModel(System.Windows.Window window)
          => new PulseOutSignalLocatorViewModel(ViewName, _groups,
                                           SignalStore,
                                           CreatePulseOutGroupSignal, window);

        private PulseOutSignalLocatorViewModel CreateLocatorViewModel() 
            => new PulseOutSignalLocatorViewModel(ViewName, new CloseModalNavigationService(ModalNavigationStore),
                                                  _groups,
                                                  SignalStore,
                                                  CreatePulseOutGroupSignal,
                                                  AddGroup);
        private PulseOutGroupSignalGroup CreatePulseOutGroupSignal(Signal signal)
        {
            if (signal.SignalName.IndexOf("_Duty") > -1 || signal.SignalName.IndexOf("_Freq") > -1)
            {
                string[] groupName = signal.SignalName.Split(new string[] { "_Duty", "_Freq" }, StringSplitOptions.RemoveEmptyEntries);
                if (groupName.Length == 1)
                {
                    var group = new PulseOutGroupSignalGroup(groupName[0]);
                    var existSignal = SignalStore.Signals.FirstOrDefault(x => x.Name == signal.SignalName && x.MessageID == signal.MessageID);
                    PulseOutGroupSignal pulseInSignal;
                    if (existSignal == null || !(existSignal is PulseOutGroupSignal analog))
                    {
                        pulseInSignal = new PulseOutGroupSignal(signal, VIEWNAME, groupName[0]);
                        SignalStore.AddSignal(pulseInSignal);
                    }
                    else
                    {
                        pulseInSignal = existSignal as PulseOutGroupSignal;
                    }
                    if (signal.SignalName.IndexOf("_Duty") > -1)
                    {
                        group.DutyCycle = pulseInSignal;
                    }
                    else
                    {
                        group.Freq = pulseInSignal;
                    }
                    return group;
                }
            }
            return null;
        }

        private void AddGroup(ObservableCollection<PulseOutGroupSignalGroup> groups, PulseOutGroupSignalGroup group)
        {
            var existGroup = groups.FirstOrDefault(x => x.GroupName == group.GroupName);
            if (existGroup != null)
            {
                if (group.Freq != null)
                    existGroup.Freq = group.Freq;
                if (group.DutyCycle != null)
                    existGroup.DutyCycle = group.DutyCycle;
            }
            else
            {
                groups.Add(group);
            }
        }
        #endregion

        private void GetGroups(IEnumerable<PulseOutGroupSignal> pulseOutGroupsignals)
        {
            //var gdicSignals = SignalStore.GetSignals<PulseOutGroupSignal>();

             pulseOutGroupsignals
            .GroupBy(s => s.GroupName)
            .Select(g =>
            {
                if (!string.IsNullOrEmpty(g.Key))
                {
                    var group = new PulseOutGroupSignalGroup(g.Key);
                    var signals = g.ToList();
                    signals.Sort((x, y) =>
                    {
                        return x.Name.CompareTo(y.Name);
                    });
                    group.DutyCycle = signals[0];
                    group.Freq = signals[1];
                    group.UpdateViewEnable(VIEWNAME);
                    return group;
                }
                return null;
            })
            .OrderBy(x=>x.GroupName)
            .ToList()
            .ForEach(x =>
            {
                _groups.Add(x);
            });
            //gDICStatusGroups.Sort
        }

        private void Update()
        {
            foreach (var group in Groups)
            {
                group.Freq.UpdateRealValue();
                group.DutyCycle.UpdateRealValue();
            }
            UVW_PWM_Freq.UpdateRealValue();
            PWM_U_Duty.UpdateRealValue();
            PWM_V_Duty.UpdateRealValue();
            PWM_W_Duty.UpdateRealValue();
            UVW_PWM_Polarity.UpdateRealValue();
            UVW_PWM_Dead_Time.UpdateRealValue();

            Send();
        }
       

        #region Fixed Signals
        public PulseOutSingleSignal PWM_U_Duty => SignalStore.GetSignals<PulseOutSingleSignal>().FirstOrDefault(x => x.Name == "PWM_U_Duty");
        public PulseOutSingleSignal PWM_V_Duty => SignalStore.GetSignals<PulseOutSingleSignal>().FirstOrDefault(x => x.Name == "PWM_V_Duty");
        public PulseOutSingleSignal PWM_W_Duty => SignalStore.GetSignals<PulseOutSingleSignal>().FirstOrDefault(x => x.Name == "PWM_W_Duty");
        public PulseOutSingleSignal UVW_PWM_Freq => SignalStore.GetSignals<PulseOutSingleSignal>().FirstOrDefault(x => x.Name == "UVW_PWM_Freq");
        public PulseOutSingleSignal UVW_PWM_Polarity => SignalStore.GetSignalByName<PulseOutSingleSignal>("UVW_PWM_Polarity", true);
        public PulseOutSingleSignal UVW_PWM_Dead_Time => SignalStore.GetSignalByName<PulseOutSingleSignal>("UVW_PWM_Dead_Time", true);
        #endregion
    }
}
