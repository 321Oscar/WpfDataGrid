﻿using CommunityToolkit.Mvvm.ComponentModel;
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
        [ObservableProperty]
        public double MaxThreshold { get; set; }
        [ObservableProperty]
        public double MinThreshold { get; set; }

        public IEnumerable<AnalogSignal> AnalogSignals => SignalStore.GetSignals<AnalogSignal>();

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
        private void UpdateSignalThreshold(double max,double min)
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

        protected override void CurrentDevice_OnMsgReceived(IEnumerable<IFrame> frames)
        {
            foreach (var signal in SignalStore.ParseMsgsYield(frames, AnalogSignals))
            {
                if (signal != null)
                    LogService.Info($"{signal.Name}:{signal.RealValue}");
            }

        }
    }
}
