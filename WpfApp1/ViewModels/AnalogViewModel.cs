using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WpfApp1.Devices;
using WpfApp1.Models;
using WpfApp1.Services;
using WpfApp1.Stores;

namespace WpfApp1.ViewModels
{
    public class AnalogViewModel : ViewModelBase
    {
        private RelayCommand _updateSignalThresholdCommand;
        private RelayCommand _resetSignalThresholdCommand;
        private readonly ObservableCollection<AnalogSignal> _signals = new ObservableCollection<AnalogSignal>();

        public AnalogViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService, 
            ModalNavigationStore modalNavigationStore, IServiceProvider serviceProvider)
            : base(signalStore, deviceStore, logService, modalNavigationStore, serviceProvider)
        {
            
        }

        ~AnalogViewModel()
        {
            
        }

        public override void Dispose()
        {
            SignalStore.SaveViewSignalLocator(nameof(AnalogViewModel), _signals);
        }

        public override void Init()
        {
            //base.Init();
            foreach(var signal in SignalStore.SignalLocatorInfo.GetViewSignalInfo(nameof(AnalogViewModel)).Signals.OfType<AnalogSignal>())
            {
                _signals.Add(signal);
            }

            //await Task.Delay(1000);
        }

        public AnalogSignal CurrentAnalogSignal { get; set; }

        public bool UpdateAll { get; set; }

        public double MaxThreshold { get; set; }

        public double MinThreshold { get; set; }

        public IEnumerable<AnalogSignal> AnalogSignals
        {
            get
            {
                //System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                //stopwatch.Restart();
                //return SignalStore.GetSignals<AnalogSignal>(nameof(AnalogViewModel));
                return _signals;
                //stopwatch.Stop();
                //Console.WriteLine($"{DateTime.Now:HH:mm:ss fff} analog get data take {stopwatch.ElapsedMilliseconds} ms");
                //return signals;
            }
        }

        public ICommand UpdateSignalThresholdCommand { get => _updateSignalThresholdCommand ?? (_updateSignalThresholdCommand = new RelayCommand(UpdateSignalThreshold)); }
        public ICommand ResetSignalThresholdCommand { get => _resetSignalThresholdCommand ?? (_resetSignalThresholdCommand = new RelayCommand(ResetSignalThreshold)); }
        public ICommand LoadSignalThresholdCommand { get; }
        public ICommand SaveSignalThresholdCommand { get; }
        public ICommand CalculateSignalStdDevCommand { get; }
       
        private void ResetSignalThreshold()
        {
            UpdateSignalThreshold(5, 0);
        }

        private void UpdateSignalThreshold()
        {
            UpdateSignalThreshold(MaxThreshold, MinThreshold);
        }
        private void UpdateSignalThreshold(double max, double min)
        {
            if (UpdateAll)
            {
                foreach (var signal in AnalogSignals)
                {
                    if(signal is AnalogSignal analogSignal)
                    {
                        analogSignal.MinThreshold = min;
                        analogSignal.MaxThreshold = max;
                    }
                }
            }
            else
            {
                if (CurrentAnalogSignal != null)
                {
                    CurrentAnalogSignal.MinThreshold = min;
                    CurrentAnalogSignal.MaxThreshold = max;
                }
                else
                {
                    //do nothing
                }
            }

        }

        public override void LocatorSignals()
        {
            ModalNavigationService<AnalogSignalLocatorViewModel> modalNavigationService =
                new ModalNavigationService<AnalogSignalLocatorViewModel>(
                    this.ModalNavigationStore,
                    () => new AnalogSignalLocatorViewModel(new CloseModalNavigationService(ModalNavigationStore), _signals, SignalStore,
                    (signal) =>
                    {
                        var existSignal = SignalStore.Signals.FirstOrDefault(x => x.Name == signal.SignalName && x.MessageID == signal.MessageID);
                        if (existSignal != null && existSignal is AnalogSignal analog)
                            return analog;

                        AnalogSignal analogSignal = new AnalogSignal()
                        {
                            Name = signal.SignalName,
                            StartBit = (int)signal.startBit,
                            Factor = signal.factor,
                            Offset = signal.offset,
                            ByteOrder = (int)signal.byteOrder,
                            Length = (int)signal.signalSize,
                            MessageID = signal.MessageID,

                        };
                        analogSignal.ViewName += "Analog";
                        analogSignal.PinNumber = signal.Comment.GetCommentByKey("Pin_Number");
                        analogSignal.ADChannel = signal.Comment.GetCommentByKey("A/D_Channel");
                        analogSignal.Transform2Type = (int)signal.Comment.GetCommenDoubleByKey("Conversion_mode", 0);
                        if (analogSignal.Transform2Type == 0)
                        {
                            analogSignal.TransForm2Factor = signal.Comment.GetCommenDoubleByKey("Factor", 1);
                            analogSignal.TransForm2Offset = signal.Comment.GetCommenDoubleByKey("Offset", 0);
                        }
                        else
                        {
                            analogSignal.TableName = signal.Comment.GetCommentByKey("Table");
                        }
                        SignalStore.AddSignal(analogSignal);
                        return analogSignal;
                    }));
            modalNavigationService.Navigate();
        }
    }
}
