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
using WpfApp1.Models;
using WpfApp1.Services;
using WpfApp1.Stores;

namespace WpfApp1.ViewModels
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
        private IEnumerable<GDICStatusRegisterGroup> gDICStatusRegisterGroups;
        private GDICStatusGroup currentGDICStatusGroup;
        private RelayCommand<GDICStatusGroup> _WriteStatusCommand;

        public IEnumerable<GDICStatusRegisterGroup> GDICStatusGroups => gDICStatusRegisterGroups;
        public GDICStatusGroup CurrentGDICStatusGroup
        {
            get => currentGDICStatusGroup;
            set => SetProperty(ref currentGDICStatusGroup, value);
        }

        public ICommand WriteStatusCommand { get => _WriteStatusCommand ?? (_WriteStatusCommand = new RelayCommand<GDICStatusGroup>(WriteStatus)); }
        private void GetGDICStatusGroups()
        {
            var gdicSignals = SignalStore.GetSignals<GDICStatusDataSignal>();

            var gdicStatusGroups = gdicSignals.GroupBy(s => s.GroupName)
                                              .Select(g =>
                                              {
                                                  var classRoom = new GDICStatusGroup(g.Key);
                                                  var signals = g.OrderByDescending(x => x.StartBit).ToList();
                                                  int length = 10;
                                                  //int x = signals.Count - length;
                                                  for (int i = signals.Count - 1; i > -1; i--)
                                                  {
                                                      int idenx = i + (length - signals.Count);

                                                      classRoom.GDICStatusSignals[idenx] = signals[i];
                                                  }
                                                  //classRoom.GDICStatusSignals.AddRange(signals);
                                                  return classRoom;
                                              })
                                              .OrderBy(x => x.GroupName)
                                              .ToList();

            gDICStatusRegisterGroups = gdicStatusGroups.GroupBy(s => s.RegisterName)
                                                  .Select(g =>
                                                  {
                                                      var registerGroup = new GDICStatusRegisterGroup(g.Key);
                                                      var signals = g.ToList();

                                                      //signal length should be 6 (4 Input status and 1 Write and 1 writeIndex)
                                                      registerGroup.WriteStatus = signals.FirstOrDefault(x => x.InOrOut &&
                                                        x.GroupName.IndexOf("status", StringComparison.OrdinalIgnoreCase) > -1);
                                                      registerGroup.WriteStatus.WriteIndex = signals.FirstOrDefault(x => x.InOrOut &&
                                                        x.GroupName.IndexOf("register", StringComparison.OrdinalIgnoreCase) > -1).GDICStatusSignals.FirstOrDefault(x => x != null);
                                                      var inStatus = signals.Where(x => x.InOrOut == false).OrderBy(x => x.Startbit).ToArray();

                                                      for (int i = 0; i < inStatus.Count(); i++)
                                                      {
                                                          registerGroup.GDICStatusGroups[i] = inStatus[i];
                                                      }

                                                      return registerGroup;
                                                  });
            //gDICStatusGroups.Sort
        }


        private void WriteStatus(GDICStatusGroup obj)
        {
            //throw new NotImplementedException();
            Send(SignalStore.BuildFrames(SignalStore.GetSignals<GDICStatusDataSignal>().Where(x => x.InOrOut)));
        }

        //--------------------------------------------------------------------

        //--Aout--------------------------------------------------------------
        private ObservableCollection<GDICAoutTemperatureSignal> temperatueSignals = new ObservableCollection<GDICAoutTemperatureSignal>();
        private ObservableCollection<GDICAoutTemperatureSignal> temperatueAoutSignals = new ObservableCollection<GDICAoutTemperatureSignal>();
        private ObservableCollection<GDICAoutSignal> deviceSelections = new ObservableCollection<GDICAoutSignal>();
        private GDICAoutTemperatureSignal currentDevice;
        private RelayCommand _updateThresholdCommand;
        public IEnumerable<GDICAoutTemperatureSignal> TemperatueSignals => temperatueSignals;
        public IEnumerable<GDICAoutTemperatureSignal> TemperatueAoutSignals => temperatueAoutSignals;

        private void GetTemperatueSignals()
        {
            var gdicSignals = SignalStore.GetSignals<GDICAoutSignal>();

            var allTemp = gdicSignals.GroupBy(s => s.GDDevice + s.Selection + s.CanChangeSelection)
                .Select(g =>
                {
                    var signals = g.ToList();
                    var dutyCanChangeSelection = signals.FirstOrDefault(x => x.Name.IndexOf("Duty") > -1);
                    //var dutyCanNotChangeSelection = signals.FirstOrDefault(x => x.Name.IndexOf("Duty") > -1 && !x.CanChangeSelection);
                    var freqCanChangeSelection = signals.FirstOrDefault(x => x.Name.IndexOf("Freq") > -1);
                    //var freqCanNotChangeSelection = signals.FirstOrDefault(x => x.Name.IndexOf("freq") > -1 && !x.CanChangeSelection);

                    GDICAoutTemperatureSignal temperature = new GDICAoutTemperatureSignal(dutyCanChangeSelection, freqCanChangeSelection);

                    return temperature;
                });
            deviceSelections.AddRange(gdicSignals.Where(x => x.Name.IndexOf("Select", StringComparison.OrdinalIgnoreCase) > -1));
            temperatueSignals.AddRange(allTemp.Where(x => !x.CanChangeSelection));
            temperatueAoutSignals.AddRange(allTemp.Where(x => x.CanChangeSelection));
        }

        public IEnumerable<GDICAoutSignal> DeviceSelections { get => deviceSelections; }
        public GDICAoutTemperatureSignal CurrentDevice
        {
            get => currentDevice;
            set
            {
                currentDevice = value;
                DeviceSelections.ToList().ForEach(
                    x =>
                    {
                        if(x.Name.IndexOf(currentDevice.ToString(),StringComparison.OrdinalIgnoreCase) > -1)
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
        private string currentAmux;

        public string CurrentAmux
        {
            get => currentAmux; 
            set
            {
                currentAmux = value;

                if (CurrentDevice != null)
                {
                    //foreach (var signal in temperatueAoutSignals)
                    //{
                    CurrentDevice.Selection = currentAmux;
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
                    Send(SignalStore.BuildFrames(DeviceSelections));
                }
            }
        }
        public double MaxThreshold { get; set; }
        public double MinThreshold { get; set; }

        public ICommand UpdateThresholdCommand { get => _updateThresholdCommand ?? (_updateThresholdCommand = new RelayCommand(UpdateThreshold)); }
        private void UpdateThreshold()
        {
            foreach (var signal in TemperatueSignals)
            {
                signal.MaxThreshold = MaxThreshold;
                signal.MinThreshold = MinThreshold;
                signal.Duty.MaxThreshold = MaxThreshold;
                signal.Duty.MinThreshold = MinThreshold;
                signal.Freq.MaxThreshold = MaxThreshold;
                signal.Freq.MinThreshold = MinThreshold;
            }

            foreach (var signal in TemperatueAoutSignals)
            {
                signal.MaxThreshold = MaxThreshold;
                signal.MinThreshold = MinThreshold;
                signal.Duty.MaxThreshold = MaxThreshold;
                signal.Duty.MinThreshold = MinThreshold;
                signal.Freq.MaxThreshold = MaxThreshold;
                signal.Freq.MinThreshold = MinThreshold;
            }
        }

        //--------------------------------------------------------------------
        //---Register---------------------------------------------------------
        private IEnumerable<GDICRegisterDeviceGroup> registers;
       
        public IEnumerable<GDICRegisterDeviceGroup> Registers => registers;

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

            registers = regesiterGroup.GroupBy(x => string.Join("-", x.GroupName.Split("-").Take(2))).Select(
                g =>
                {
                    GDICRegisterDeviceGroup d = new GDICRegisterDeviceGroup(g.Key);
                    d.RegisterGroups.AddRange(g);
                    return d;
                });
        }

        //--------------------------------------------------------------------
        //--ADC---------------------------------------------------------------
        private IEnumerable<GDICADCSignal> adcSignals;
        private string currentValueSelection;
      

        public IEnumerable<GDICADCSignal> AdcSignals => adcSignals;
        public GDICADCSignal CurrentAdcSignal { get; set; }
        public List<string> AdcValueSelections { get; } = new List<string>()
        {
            "DEST",
            "AMUXIN",
            "VCC",
            "VEE",
            "Power",
            "Die",
        };

        public string CurrentValueSelection { get=> currentValueSelection; set
            {
                currentValueSelection = value;
                if (CurrentAdcSignal != null)
                {
                    CurrentAdcSignal.WriteSignal.OriginValue = AdcValueSelections.IndexOf(currentValueSelection) + 1;

                    Send(SignalStore.BuildFrames(new SignalBase[] { CurrentAdcSignal.WriteSignal }));
                }
            }
        }
        private void LoadAdcSignals()
        {
            var gdicSignals = SignalStore.GetSignals<GDICRegisterSignal>(GDICADCViewName);

            //var inputs = gdicSignals.Where(x => !x.InOrOut);
            //var writes = gdicSignals.Where(x => x.InOrOut);

            adcSignals = gdicSignals
                .GroupBy(s=>s.DeviceName)
                .Select(g =>
                {
                    return new GDICADCSignal()
                    {
                        RegisterSignal = g.ToList().FirstOrDefault(x=> !x.InOrOut),
                        WriteSignal = g.ToList().FirstOrDefault(x=> x.InOrOut),
                    };
                });

            //adcSignals = 
        }
        //--------------------------------------------------------------------


    }
}
