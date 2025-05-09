﻿using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using ERad5TestGUI.Models;
using ERad5TestGUI.Services;
using ERad5TestGUI.Stores;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace ERad5TestGUI.ViewModels
{
    public class PPAWLViewModel : SendFrameViewModelBase
    {
        private readonly ObservableCollection<AnalogSignal> _analogSignals = new ObservableCollection<AnalogSignal>();
        //private readonly ObservableCollection<DiscreteOutputSignal> _outputSignals = new ObservableCollection<DiscreteOutputSignal>();
        //private readonly ObservableCollection<DiscreteInputSignal> _inputSignals = new ObservableCollection<DiscreteInputSignal>();
        //private readonly ObservableCollection<PulseInSignalGroup> _pulseInGroups = new ObservableCollection<PulseInSignalGroup>();

        private readonly List<SignalGroupBase> _groups = new List<SignalGroupBase>();
        private RelayCommand _locResCommand;
        private RelayCommand _updateLimitCommand;
        private AnalogSignal _elecCurrent;
        private double _elecCurrentValue;

        public PPAWLViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService) 
            : base(signalStore, deviceStore, logService)
        {

        }
        public PPAWLViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService, ModalNavigationStore modalNavigationStore, IServiceProvider serviceProvider) : base(signalStore, deviceStore, logService, modalNavigationStore, serviceProvider)
        {
           
        }
        public ICommand LocResCommand => _locResCommand ?? (_locResCommand = new RelayCommand(LocatorResSignals));
        public ICommand UpdateLimitCommand => _updateLimitCommand ?? (_updateLimitCommand = new RelayCommand(UpdateLimit));
        public IEnumerable<AnalogSignal> AnalogSignals
        {
            get => _analogSignals;
        }

        public IEnumerable<SignalGroupBase> Groups { get => _groups; }
        public PulseOutSingleSignal PPAWL_Current_Limit { get; set; }
        public PulseOutSingleSignal PPAWL_Current_Limit_Update { get; set; }

        public double ElecCurrentValue { get => _elecCurrentValue; set => SetProperty(ref _elecCurrentValue, value); }

        public override void Init()
        {
            base.Init();
            _elecCurrent = SignalStore.GetSignalByName<AnalogSignal>("PPAWL_MTR_CURRENT_SENSE_AI");
            if (_elecCurrent != null)
            {
                _elecCurrent.PropertyChanged += ElecCurrent_PropertyChanged;
            }
            LoadOtherSignals();
        }

        private void ElecCurrent_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AnalogSignal.OriginValue))
            {
                var elec = sender as AnalogSignal;
                if (elec.OriginValue < 512)
                {
                    ElecCurrentValue = 0;
                }
                else
                {
                    ElecCurrentValue = (elec.OriginValue / 4095 * 5 - 0.625) * 6.25;
                }
            }
        }

        private void LoadOtherSignals()
        {
            _analogSignals.AddRange(SignalStore.GetSignals<AnalogSignal>(ViewName));

            DiscreteInputSignalGroup disInputGroup = new DiscreteInputSignalGroup("Discrete Inputs");
            disInputGroup.Signals.AddRange(SignalStore.GetSignals<DiscreteInputSignal>(ViewName));
            _groups.Add(disInputGroup);

            DiscreteOutputSignalGroup diOutGroup = new DiscreteOutputSignalGroup("Discrete Outputs");
            diOutGroup.Signals.AddRange(SignalStore.GetSignals<DiscreteOutputSignal>(ViewName));
            _groups.Add(diOutGroup);

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

            PulseOutGroupList pulseOutGroup = new PulseOutGroupList("Pulse Out");

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
                       .ForEach(x => pulseOutGroup.Groups.Add(x));

            //add textbox
            TextBoxSignalView timeFrame = new TextBoxSignalView()
            {
                Title = "Time Frame(ms)",
                Signal = SignalStore.GetSignalByName<PulseOutSingleSignal>("PPAWL_Frame_Time", true)
            };

            CommandSignalView updateCmd = new CommandSignalView(
                new RelayCommand(() => 
                {
                    var start = SignalStore.GetSignalByName<PulseOutSingleSignal>("PPAWL_PWM_Update", true);
                    start.OriginValue = 1;
                    foreach (var item in SignalStore.GetSignals<PulseOutGroupSignal>(ViewName))
                    {
                        item.UpdateRealValue();
                    }

                    this.SendFD(SignalStore.BuildFrames(SignalStore.GetSignals<PulseOutGroupSignal>(ViewName)));
                    start.OriginValue = 0;

                }))
            {
                Title = "Update",
                Signal = SignalStore.GetSignalByName<PulseOutSingleSignal>("PPAWL_PWM_Update", true)
            };
            pulseOutGroup.AddSignalViews(timeFrame,updateCmd);
            //pulseOutGroup.SingleSignals.Add(timeFrame);
            //pulseOutGroup.UpdateCommand = new RelayCommand(() =>
            //{
            //    var outSignals = SignalStore.GetSignals<PulseOutGroupSignal>(ViewName);
            //    outSignals.ToList().ForEach(x => x.UpdateRealValue());
            //    this.SendFD(SignalStore.BuildFrames(outSignals));
            //});
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

            GDICRegistersGroup registers = new GDICRegistersGroup("DRV8705");
            var resDatas = SignalStore.GetSignals<GDICRegisterSignal>(ViewName);
            GDICRegisterSignal fixedRSVD_STAT = new GDICRegisterSignal()
            {
                Name = "RSVD_STAT",
                Fixed = true,
                Address = "0x03",
                FixedValue = "RSVD",
                Value1 = "RSVD",
            };
            registers.Signals.AddRange(resDatas);
            registers.Signals.Add(fixedRSVD_STAT);
            _groups.Add(registers);
            //var regesiterGroup = resDatas.GroupBy(x => x.GroupName)
            //   .Select(g =>
            //   {
            //       var signals = g.ToList();
            //       GDICRegisterGroup reGroup = new GDICRegisterGroup(g.Key,
            //           signals.FirstOrDefault(x => x.Name.IndexOf("Data") > -1),
            //           signals.FirstOrDefault(x => x.Name.IndexOf("CRC") > -1)
            //           );

            //       return reGroup;
            //   });
            PPAWL_Current_Limit = SignalStore.GetSignalByName<PulseOutSingleSignal>("PPAWL_Current_Limit", true);
            PPAWL_Current_Limit_Update = SignalStore.GetSignalByName<PulseOutSingleSignal>("PPAWL_Current_Limit_Update", true);
        }

        private void UpdateLimit()
        {
            PPAWL_Current_Limit_Update.OriginValue = 1;

            SendFD(SignalStore.BuildFrames(new SignalBase[] { PPAWL_Current_Limit, PPAWL_Current_Limit_Update }));

            PPAWL_Current_Limit_Update.OriginValue = 0;
        }

        public override void LocatorSignals()
        {
            if (ModalNavigationStore != null)
            {
                //var modalNavigationService = new ModalNavigationService<GDICStatusDataSignalLocatorViewModel>(this.ModalNavigationStore, CreateSignalLocatorViewModel);
                //modalNavigationService.Navigate();
            }
            else
            {
                //Log("123");
                Views.DialogView dialogView = new Views.DialogView();
                var locatorViewModel = CreateSignalLocatorViewModel(dialogView);
                dialogView.DialogViewModel = locatorViewModel;
                dialogView.ShowDialog();
            }
        } 
        
        private void LocatorResSignals()
        {
            if (ModalNavigationStore != null)
            {
                //var modalNavigationService = new ModalNavigationService<GDICStatusDataSignalLocatorViewModel>(this.ModalNavigationStore, CreateSignalLocatorViewModel);
                //modalNavigationService.Navigate();
            }
            else
            {
                //Log("123");
                Views.DialogView dialogView = new Views.DialogView();
                var locatorViewModel = CreateReSignalLocatorViewModel(dialogView);
                dialogView.DialogViewModel = locatorViewModel;
                dialogView.ShowDialog();
            }
        }

        private GDICStatusDataSignalLocatorViewModel CreateSignalLocatorViewModel(System.Windows.Window window)
            => new GDICStatusDataSignalLocatorViewModel(ViewName, null,
                                                SignalStore,
                                                CreateGDICStatusByDBCSignal,
                                                window);
        private GDICRegisterSignalLocatorViewModel CreateReSignalLocatorViewModel(System.Windows.Window window)
            => new GDICRegisterSignalLocatorViewModel(ViewName, null,
                                                SignalStore,
                                                CreateRegisterByDBCSignal,
                                                window);

        private GDICStatusDataSignal CreateGDICStatusByDBCSignal(Signal signal)
        {
            //throw new NotImplementedException();
            var existSignal = SignalStore.Signals.FirstOrDefault(x => x.Name == signal.SignalName && x.MessageID == signal.MessageID);
            if (existSignal != null && existSignal is GDICStatusDataSignal analog)
                return analog;

            GDICStatusDataSignal analogSignal = new GDICStatusDataSignal(signal, ViewName);
            SignalStore.AddSignal(analogSignal);
            return analogSignal;
        }
         private GDICRegisterSignal CreateRegisterByDBCSignal(Signal signal)
        {
            //throw new NotImplementedException();
            var existSignal = SignalStore.Signals.FirstOrDefault(x => x.Name == signal.SignalName && x.MessageID == signal.MessageID);
            if (existSignal != null && existSignal is GDICRegisterSignal analog)
                return analog;

            GDICRegisterSignal analogSignal = new GDICRegisterSignal(signal, ViewName);
            SignalStore.AddSignal(analogSignal);
            return analogSignal;
        }


        //Register
    }
}