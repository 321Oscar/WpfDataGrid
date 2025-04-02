using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ERad5TestGUI.Models;
using ERad5TestGUI.Services;
using ERad5TestGUI.Stores;

namespace ERad5TestGUI.ViewModels
{
    public class GDICViewModel : SendFrameViewModelBase
    {
        public const string GDICStatusViewName = "GDIC_Status";
        public const string GDICAoutViewName = "GDIC_Aout";
        public const string GDICRegisterViewName = "GDIC_Register";
        public const string GDICADCViewName = "GDIC_ADC";

        public GDICViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService, ModalNavigationStore modalNavigationStore, IServiceProvider serviceProvider)
            : base(signalStore, deviceStore, logService, modalNavigationStore, serviceProvider)
        {
            //_signalStore = signalStore;

        }

        public GDICViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService)
            : base(signalStore, deviceStore, logService)
        {
        }

        public override void Init()
        {
            Task.Run(() =>
            {
                IsLoading = true;
                GetGDICStatusGroups();
                GetTemperatueSignals();
                LoadRegister();
                LoadAdcSignals();
            }).ContinueWith((x) =>
            {
                IsLoading = false;
            });
           
        }

        public override void Send()
        {
            //Send(SignalStore.BuildFrames(SignalStore.GetSignals<DiscreteOutputSignal>(nameof(DiscreteViewModel))));
        }
      
        //--Status-------------------------------------------------------------
        private readonly ObservableCollection<GDICStatusRegisterGroup> _gDICStatusRegisterGroups = new ObservableCollection<GDICStatusRegisterGroup>();
        private GDICStatusGroup _currentGDICStatusGroup;
        private RelayCommand<GDICStatusGroup> _writeStatusCommand;
        private AsyncRelayCommand _addStatusCommand;
        private RelayCommand _clearStatusCommand;

        public IEnumerable<GDICStatusRegisterGroup> GDICStatusGroups => _gDICStatusRegisterGroups;
        public GDICStatusGroup CurrentGDICStatusGroup
        {
            get => _currentGDICStatusGroup;
            set => SetProperty(ref _currentGDICStatusGroup, value);
        }

        public ICommand WriteStatusCommand { get => _writeStatusCommand ?? (_writeStatusCommand = new RelayCommand<GDICStatusGroup>(WriteStatus)); }
        public ICommand AddStatusCommand { get => _addStatusCommand ?? (_addStatusCommand = new AsyncRelayCommand(AddStatus, () => _gDICStatusRegisterGroups.Count == 0)); }
        public ICommand ClearStatusCommand { get => _clearStatusCommand ?? (_clearStatusCommand = new RelayCommand(ClearStatus, () => _gDICStatusRegisterGroups.Count > 0)); }
        private void GetGDICStatusGroups()
        {
           
            //gDICStatusGroups.Sort
        }
        private Task AddStatus()
        {
            var gdicSignals = SignalStore.GetSignals<GDICStatusDataSignal>(ViewName);

            var gdicStatusGroups = gdicSignals.GroupBy(s => s.GroupName)
                                              .Select(g =>
                                              {
                                                  var statusGroup = new GDICStatusGroup(g.Key);
                                                  var signals = g.OrderByDescending(x => x.StartBit).Where(x => x.Name.IndexOf("Flag") < 0).ToList();
                                                  int length = 10;
                                                  statusGroup.WriteFlag = g.FirstOrDefault(x => x.Name.IndexOf("Flag") > -1);
                                                  //int x = signals.Count - length;
                                                  for (int i = signals.Count - 1; i > -1; i--)
                                                  {
                                                      int idenx = i + (length - signals.Count);

                                                      statusGroup.GDICStatusSignals[idenx] = signals[i];
                                                  }
                                                  //classRoom.GDICStatusSignals.AddRange(signals);
                                                  return statusGroup;
                                              })
                                              .OrderBy(x => x.GroupName)
                                              .ToList();

            var registerGroups = gdicStatusGroups.GroupBy(s => s.RegisterName)
                                                  .Select(g =>
                                                  {
                                                      var registerGroup = new GDICStatusRegisterGroup(g.Key);
                                                      var statusGroups = g.ToList();

                                                      //signal length should be 6 (4 Input status and 1 Write and 1 writeIndex)
                                                      registerGroup.WriteStatus = statusGroups.FirstOrDefault(x => x.InOrOut &&
                                                        x.GroupName.IndexOf("status", StringComparison.OrdinalIgnoreCase) > -1);
                                                      registerGroup.WriteStatus.WriteIndex = statusGroups.FirstOrDefault(x => x.InOrOut &&
                                                         x.GroupName.IndexOf("register", StringComparison.OrdinalIgnoreCase) > -1).
                                                         GDICStatusSignals.FirstOrDefault(x => x != null);
                                                      var inStatus = statusGroups.Where(x => x.InOrOut == false).OrderBy(x => x.Startbit).ToArray();

                                                      for (int i = 0; i < inStatus.Count(); i++)
                                                      {
                                                          registerGroup.GDICStatusGroups[i] = inStatus[i];
                                                      }

                                                      return registerGroup;
                                                  });
            IsLoading = true;
            return Task.Run(() =>
            {
                foreach (var g in registerGroups)
                {
                    Dispatch(() => _gDICStatusRegisterGroups.Add(g));
                }
                
            }).ContinueWith((x) =>
            {
                IsLoading = false;
                Dispatch(RaiseCommandCanExecute);
            });
        }
       
        private void ClearStatus()
        {
            _gDICStatusRegisterGroups.Clear();
            RaiseCommandCanExecute();
        }

        private void RaiseCommandCanExecute()
        {
            _addStatusCommand.NotifyCanExecuteChanged();
            _clearStatusCommand.NotifyCanExecuteChanged();
        }

        private void WriteStatus(GDICStatusGroup obj)
        {
            //throw new NotImplementedException();
            obj.WriteFlag.OriginValue = 1;
            SendFD(SignalStore.BuildFrames(SignalStore.GetSignals<GDICStatusDataSignal>().Where(x => x.InOrOut)));
            obj.WriteFlag.OriginValue = 0;
        }

        //--------------------------------------------------------------------

        //--Aout--------------------------------------------------------------
        private readonly ObservableCollection<GDICAoutTemperatureSignal> _temperatueSignals = new ObservableCollection<GDICAoutTemperatureSignal>();
        private readonly ObservableCollection<GDICAoutTemperatureSignal> _temperatueAoutSignals = new ObservableCollection<GDICAoutTemperatureSignal>();
        private readonly ObservableCollection<GDICAoutSignal> _deviceSelections = new ObservableCollection<GDICAoutSignal>();
        private GDICAoutTemperatureSignal _currentDevice;
        private RelayCommand _updateThresholdCommand;
        private RelayCommand _calculateSTDCommand;
        public IEnumerable<GDICAoutTemperatureSignal> TemperatueSignals => _temperatueSignals;
        public IEnumerable<GDICAoutTemperatureSignal> TemperatueAoutSignals => _temperatueAoutSignals;

        private void GetTemperatueSignals()
        {
            var gdicSignals = SignalStore.GetSignals<GDICAoutSignal>();
            _deviceSelections.AddRange(gdicSignals.Where(x => x.Name.IndexOf("Select", StringComparison.OrdinalIgnoreCase) > -1));

            var allTemp = gdicSignals.GroupBy(s => s.GDDevice + s.Selection + s.CanChangeSelection)
                .Select(g =>
                {
                    var signals = g.ToList();
                    var duty = signals.FirstOrDefault(x => x.Name.IndexOf("Duty") > -1);
                    //var dutyCanNotChangeSelection = signals.FirstOrDefault(x => x.Name.IndexOf("Duty") > -1 && !x.CanChangeSelection);
                    var freq = signals.FirstOrDefault(x => x.Name.IndexOf("Freq") > -1);
                    //var freqCanNotChangeSelection = signals.FirstOrDefault(x => x.Name.IndexOf("freq") > -1 && !x.CanChangeSelection);

                    var temperature = new GDICAoutTemperatureSignal(duty, freq, !duty.CanChangeSelection);

                    return temperature;
                });
            _temperatueSignals.AddRange(allTemp.Where(x => !x.CanChangeSelection));
            _temperatueAoutSignals.AddRange(allTemp.Where(x => x.CanChangeSelection));
        }

        public IEnumerable<GDICAoutSignal> DeviceSelections { get => _deviceSelections; }
        public GDICAoutTemperatureSignal CurrentDevice
        {
            get => _currentDevice;
            set
            {
                _currentDevice = value;
                DeviceSelections.ToList().ForEach(
                    x =>
                    {
                        if(x.Name.IndexOf(_currentDevice.ToString(),StringComparison.OrdinalIgnoreCase) > -1)
                        {
                            x.OriginValue = AmuxSelections.IndexOf(CurrentAmux);
                        }
                        else
                        {
                            x.OriginValue = 0;
                        }
                    });
            }
        }
        public List<string> AmuxSelections { get; } = new List<string>()
        {
            "None",
            "DEST",
            "AMUXN",
            "VCC",
            "VEE",
        };
        private string _currentAmux;

        public string CurrentAmux
        {
            get => _currentAmux; 
            set
            {
                _currentAmux = value;

                if (CurrentDevice != null)
                {
                    //foreach (var signal in temperatueAoutSignals)
                    //{
                    CurrentDevice.Selection = _currentAmux;
                    //CurrentDevice.Duty.Selection = currentDevice.DisplayName;
                    //CurrentDevice.Freq.Selection = currentDevice.DisplayName;
                    //}
                    DeviceSelections.ToList().ForEach(
                    x =>
                    {
                        if (x.GDDevice.IndexOf(CurrentDevice.ToString(), StringComparison.OrdinalIgnoreCase) > -1)
                        {
                            x.OriginValue = AmuxSelections.IndexOf(CurrentAmux);
                        }
                        else
                        {
                            x.OriginValue = 0;
                        }
                    });

                    //send meesage
                    SendFD(SignalStore.BuildFrames(DeviceSelections));
                }
            }
        }
        public double MaxThreshold { get; set; }
        public double MinThreshold { get; set; }
        public int ThresholdType { get; set; }
        public ICommand UpdateThresholdCommand { get => _updateThresholdCommand ?? (_updateThresholdCommand = new RelayCommand(UpdateThreshold)); }
        private void UpdateThreshold()
        {
            UpdateThresholds(TemperatueSignals);

            UpdateThresholds(TemperatueAoutSignals);
        }

        private void UpdateThresholds(IEnumerable<GDICAoutTemperatureSignal> temperatureSignals)
        {
            foreach (var signal in temperatureSignals)
            {
                if (ThresholdType == 0)
                {
                    signal.MaxThreshold = MaxThreshold;
                    signal.MinThreshold = MinThreshold;
                }
                else if (ThresholdType == 1)
                {
                    signal.Duty.MaxThreshold = MaxThreshold;
                    signal.Duty.MinThreshold = MinThreshold;
                }
                else if (ThresholdType == 2)
                {
                    signal.Freq.MaxThreshold = MaxThreshold;
                    signal.Freq.MinThreshold = MinThreshold;
                }
            }
        }

        public int StandardCount { get; set; } = 1; 
        public ICommand CalculateSTDCommand => _calculateSTDCommand ?? (_calculateSTDCommand = new RelayCommand(CalculateSTD));
        private void CalculateSTD()
        {
            foreach (var signal in TemperatueSignals)
            {
                signal.CalStandard(StandardCount);
            }
            foreach (var signal in TemperatueAoutSignals)
            {
                signal.CalStandard(StandardCount);
            }
        }

        //--------------------------------------------------------------------
        //---Register---------------------------------------------------------
        private readonly ObservableCollection<GDICRegisterDeviceGroup> _registers = new ObservableCollection<GDICRegisterDeviceGroup>();
       
        public IEnumerable<GDICRegisterDeviceGroup> Registers => _registers;

        private void LoadRegister()
        {
            var gdicSignals = SignalStore.GetSignals<GDICRegisterSignal>(GDICRegisterViewName);

            var regesiterGroup = gdicSignals.GroupBy(x => x.GroupName)
                .Select(g =>
                {
                    var signals = g.ToList();
                    GDICRegisterGroup reGroup = new GDICRegisterGroup(g.Key,
                        signals.FirstOrDefault(x=>x.Name.IndexOf("Data") > -1),
                        signals.FirstOrDefault(x=>x.Name.IndexOf("CRC") > -1)
                        );

                    return reGroup;
                });

            _registers.AddRange( regesiterGroup.GroupBy(x => string.Join("-", x.GroupName.Split("-").Take(2))).Select(
                g =>
                {
                    GDICRegisterDeviceGroup d = new GDICRegisterDeviceGroup(g.Key);
                    d.RegisterGroups.AddRange(g);
                    return d;
                }));
        }

        //--------------------------------------------------------------------
        //--ADC---------------------------------------------------------------
        private readonly ObservableCollection<GDICADCSignal> _adcSignals = new ObservableCollection<GDICADCSignal>();
        private readonly ObservableCollection<GDICRegisterADCGroup> _adcSignalGroups = new ObservableCollection<GDICRegisterADCGroup>();
        private string _currentValueSelection;

        [Obsolete]
        public IEnumerable<GDICADCSignal> AdcSignals => _adcSignals;
        public IEnumerable<GDICRegisterADCGroup> AdcSignalGroups => _adcSignalGroups;
        [Obsolete]
        public GDICADCSignal CurrentAdcSignal { get; set; }
        [Obsolete]
        public List<string> AdcValueSelections { get; } = new List<string>()
        {
            "DEST",
            "AMUXIN",
            "VCC",
            "VEE",
            "Power",
            "Die",
        };
        [Obsolete]
        public string CurrentValueSelection 
        { 
            get=> _currentValueSelection; 
            set
            {
                _currentValueSelection = value;
                if (CurrentAdcSignal != null)
                {
                    CurrentAdcSignal.WriteSignal.OriginValue = AdcValueSelections.IndexOf(_currentValueSelection);

                    SendFD(SignalStore.BuildFrames(new SignalBase[] { CurrentAdcSignal.WriteSignal }));
                }
            }
        }
        private void LoadAdcSignals()
        {
            var gdicSignals = SignalStore.GetSignalsByName<GDICRegisterSignal>("_ADC_Data");
            var gs = gdicSignals.GroupBy(x => x.DeviceName)
                       .Select(g =>
                        {
                            GDICRegisterADCGroup group = _adcSignalGroups.FirstOrDefault(x => x.GroupName == g.Key);
                            
                            if (group == null)
                                group = new GDICRegisterADCGroup() { GroupName = g.Key };
                            var signals = g.ToArray();
                            group.DeviceName = signals[0].DeviceName;
                            group.Desat = signals.FirstOrDefault(x => x.Name.IndexOf("Desat", StringComparison.OrdinalIgnoreCase) > -1);
                            group.Amuxin = signals.FirstOrDefault(x => x.Name.IndexOf("amuxin", StringComparison.OrdinalIgnoreCase) > -1);
                            group.VCC = signals.FirstOrDefault(x => x.Name.IndexOf("vcc", StringComparison.OrdinalIgnoreCase) > -1);
                            group.VEE = signals.FirstOrDefault(x => x.Name.IndexOf("vee", StringComparison.OrdinalIgnoreCase) > -1);
                            group.PowerTemp = signals.FirstOrDefault(x => x.Name.IndexOf("power", StringComparison.OrdinalIgnoreCase) > -1);
                            group.DieTemp = signals.FirstOrDefault(x => x.Name.IndexOf("die", StringComparison.OrdinalIgnoreCase) > -1);
                            return group;
                        });
            _adcSignalGroups.AddRange(gs);
            //var inputs = gdicSignals.Where(x => !x.InOrOut);
            //var writes = gdicSignals.Where(x => x.InOrOut);

            //_adcSignals.AddRange( gdicSignals
            //    .GroupBy(s=>s.DeviceName)
            //    .Select(g =>
            //    {
            //        return new GDICADCSignal()
            //        {
            //            RegisterSignal = g.ToList().FirstOrDefault(x=> !x.InOrOut),
            //            WriteSignal = g.ToList().FirstOrDefault(x=> x.InOrOut),
            //        };
            //    }));

            //adcSignals = 
        }
        //--------------------------------------------------------------------


    }
}
