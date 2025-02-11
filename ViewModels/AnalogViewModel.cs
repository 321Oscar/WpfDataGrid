using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
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
        public AnalogViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService)
            : base(signalStore, deviceStore, logService)
        {
            //_signalStore = signalStore;
            //this.deviceStore = deviceStore;
            //this.logService = logService;
            //deviceStore.BeforeCurrentDeviceChange += DeviceStore_CurrentDeviceChange;
            //deviceStore.CurrentDeviceChanged += DeviceStore_CurrentDeviceChanged;

            UpdateSignalThresholdCommand = new RelayCommand(UpdateSignalThreshold);
            ResetSignalThresholdCommand = new RelayCommand(ResetSignalThreshold);
        }

        ~AnalogViewModel()
        {

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
                var signals = SignalStore.GetSignals<AnalogSignal>(nameof(AnalogViewModel));
                //stopwatch.Stop();
                //Console.WriteLine($"{DateTime.Now:HH:mm:ss fff} analog get data take {stopwatch.ElapsedMilliseconds} ms");
                return signals;
            }
        }

        public ICommand UpdateSignalThresholdCommand { get; }
        public ICommand ResetSignalThresholdCommand { get; }
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
                    signal.MinThreshold = min;
                    signal.MaxThreshold = max;
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
    }
}
