using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

        public AnalogViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService)
            : base(signalStore, deviceStore, logService)
        {
           
        }

        ~AnalogViewModel()
        {

        }

        public override async void Init()
        {
            //base.Init();
            foreach(var signal in SignalStore.GetSignals<AnalogSignal>(nameof(AnalogViewModel)))
            {
                _signals.Add(signal);
            }

            await Task.Delay(1000);
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
