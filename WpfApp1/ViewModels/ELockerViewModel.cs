using CommunityToolkit.Mvvm.Input;
using ERad5TestGUI.Models;
using ERad5TestGUI.Services;
using ERad5TestGUI.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERad5TestGUI.ViewModels
{
    public class ELockerViewModel : SendFrameViewModelBase
    {
        private readonly List<SignalGroupBase> _groups = new List<SignalGroupBase>();
        private readonly List<SignalGroupBase> _settingGroups = new List<SignalGroupBase>();

        public ELockerViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService) : base(signalStore, deviceStore, logService)
        {
            _ViewName = "E-Locker";
        }

        public ELockerViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService, ModalNavigationStore modalNavigationStore, IServiceProvider serviceProvider) : base(signalStore, deviceStore, logService, modalNavigationStore, serviceProvider)
        {
        }


        public IEnumerable<SignalGroupBase> Groups { get => _groups; }
        public IEnumerable<SignalGroupBase> SettingGroups { get => _settingGroups; }

        public override void Init()
        {
            if(_ViewName == null)
                _ViewName = "E-Locker";
            base.Init();
            LoadSignals();
        }

        private void LoadSignals()
        {
            AnalogSignalGroup anGroup = new AnalogSignalGroup("Analog");
            anGroup.Signals.AddRange(SignalStore.GetSignals<AnalogSignal>(ViewName));
            _groups.Add(anGroup);

            DiscreteInputSignalGroup disInputGroup = new DiscreteInputSignalGroup("Discrete Inputs");
            disInputGroup.Signals.AddRange(SignalStore.GetSignals<DiscreteInputSignal>(ViewName));
            _settingGroups.Add(disInputGroup);

            DiscreteOutputSignalGroup diOutGroup = new DiscreteOutputSignalGroup("Discrete Outputs");
            diOutGroup.Signals.AddRange(SignalStore.GetSignals<DiscreteOutputSignal>(ViewName));
            _settingGroups.Add(diOutGroup);

            PulseInGroupGroup pulseInGroupGroup = new PulseInGroupGroup("Pulse In");
            var pulseInSignals = SignalStore.GetSignals<PulseInSignal>(ViewName);
            pulseInGroupGroup.Groups.AddRange(pulseInSignals
                                              .GroupBy(s => s.GroupName)
                                              .Select(g =>
                                              {
                                                  var classRoom = new PulseInSignalGroup(g.Key);
                                                  var signals = g.ToList();
                                                  signals.Sort((x, y) =>
                                                  {
                                                      return x.Name.CompareTo(y.Name);
                                                  });
                                                  classRoom.Signal_DC = signals[0];
                                                  classRoom.Signal_Freq = signals[1];
                                                  return classRoom;
                                              })
                                              .OrderBy(x => x.GroupName));

            _groups.Add(pulseInGroupGroup);

            PulseOutGroupList pulseOutGroup = new PulseOutGroupList("Start Sequence of eLOCKER_PWM");
            PulseOutGroupList pulseOutSettingGroup = new PulseOutGroupList("setting");

            SignalStore.GetSignals<PulseOutGroupSignal>(ViewName)
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
                               return group;
                           }
                           return null;
                       })
                       .OrderBy(x => x.GroupName)
                       .ToList()
                       .ForEach(x => 
                       {
                           pulseOutSettingGroup.Groups.Add(x); 
                           pulseOutGroup.Groups.Add(x); 
                       });
            TextBoxSignalView timeFrame = new TextBoxSignalView()
            {
                Title = "100% Frame(ms)",
                Signal = SignalStore.GetSignalByName<PulseOutSingleSignal>("eLOCKER_Time", true)
            };
            
            CommandSignalView startCmd = new CommandSignalView(new RelayCommand(()=> ChangeSignal("eLOCKER_PWM_Seq_Start")))
            {
                Title = "Start",
                Signal = SignalStore.GetSignalByName<PulseOutSingleSignal>("eLOCKER_PWM_Seq_Start", true)
            };
            CommandSignalView updateCmd = new CommandSignalView(new RelayCommand(()=> ChangeSignal("eLOCKER_PWM_Update")))
            {
                Title = "Update",
                Signal = SignalStore.GetSignalByName<PulseOutSingleSignal>("eLOCKER_PWM_Update", true)
            };
            CommandSignalView stopCmd = new CommandSignalView(new RelayCommand(()=> ChangeSignal("eLOCKER_PWM_Stop")))
            {
                Title = "Stop",
                Signal = SignalStore.GetSignalByName<PulseOutSingleSignal>("eLOCKER_PWM_Stop", true)
            };

            pulseOutGroup.AddSignalViews(timeFrame, startCmd, stopCmd);
            pulseOutSettingGroup.AddSignalViews(updateCmd,stopCmd);


            _settingGroups.Add(pulseOutSettingGroup);
            _groups.Add(pulseOutGroup);

            GDICStatuGroupGroup sentSignals = new GDICStatuGroupGroup("SENT");
            var sentDatas = SignalStore.GetSignals<GDICStatusDataSignal>(ViewName);
            var xx = sentDatas
                                .GroupBy(s => s.GroupName)
                                .Select(g =>
                                {
                                    int length = 6;
                                    var classRoom = new GDICStatusGroup(g.Key, length);
                                    var signals = g.OrderByDescending(x => x.StartBit).ToList();
                                    for (int i = signals.Count - 1; i > -1; i--)
                                    {
                                        int idenx = i + (length - signals.Count);

                                        classRoom.GDICStatusSignals[idenx] = signals[i];
                                    }

                                    return classRoom;
                                });
            sentSignals.Groups.AddRange(xx);
            _groups.Add(sentSignals);

            //GDICRegistersGroup registers = new GDICRegistersGroup("DRV8705");
            //var resDatas = SignalStore.GetSignals<GDICRegisterSignal>(ViewName);
            //registers.Signals.AddRange(resDatas);
            //_groups.Add(registers);
            TLF35584_Current_State = SignalStore.GetSignalByName<SPISignal>("TLF35584_Current_State");
        }
        public SPISignal TLF35584_Current_State { get; set; }

        private void ChangeSignal(string signalName)
        {
            var start = SignalStore.GetSignalByName<PulseOutSingleSignal>(signalName, true);
            start.PropertyChanged += Start_PropertyChanged;
            start.OriginValue = 1;
            start.PropertyChanged -= Start_PropertyChanged;
            start.OriginValue = 0;
        }

        private void Start_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SignalBase.OriginValue))
            {
                foreach (var item in SignalStore.GetSignals<PulseOutGroupSignal>(ViewName))
                {
                    item.UpdateRealValue();
                }

                this.SendFD(SignalStore.BuildFrames(SignalStore.GetSignals<PulseOutGroupSignal>(ViewName)));
            }
        }
    }
}
