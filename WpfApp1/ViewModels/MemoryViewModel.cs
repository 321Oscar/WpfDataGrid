﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
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
    public class MemoryViewModel : ViewModelBase
    {
        private ICommand _loadSrecFileCommand;
        private ObservableCollection<UDS.SRecord.SrecData> _s19Records = new ObservableCollection<UDS.SRecord.SrecData>();
        public MemoryViewModel(DeviceStore deviceStore, LogService logService) : base(null, deviceStore, logService)
        {
            S19Records = new ReadOnlyObservableCollection<UDS.SRecord.SrecData>(_s19Records);
        }
        public MemoryViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService, ModalNavigationStore modalNavigationStore, IServiceProvider serviceProvider) 
            : base(signalStore, deviceStore, logService, modalNavigationStore, serviceProvider)
        {
        }

        public ICommand LoadSrecFileCommand => _loadSrecFileCommand ?? (_loadSrecFileCommand = new AsyncRelayCommand(LoadSrecFile));
        public ReadOnlyObservableCollection<UDS.SRecord.SrecData> S19Records { get; set; }
        private Task LoadSrecFile()
        {
            return Task.Run(() =>
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "srec file|*.srec;*.s19;*.s28;*.s37|all|*.*";

                if (ofd.ShowDialog() == true)
                {
                    //S19RecordFile s19File = new S19RecordFile();
                    //S19RecordFile.ParseS19File(ofd.FileName, S19Records);
                    IsLoading = true;
                    UDS.SRecord.SrecFile f = new UDS.SRecord.SrecFile(ofd.FileName);
                    Dispatch(() =>
                    {
                        _s19Records.Clear();
                        foreach (var sData in f.Content)
                        {
                            _s19Records.Add(sData);
                        }
                    });
                }
                return;
            }).ContinueWith((x) =>
            {
                IsLoading = false;
            });
        }
    }

    public class ResolverViewModel : ViewModelBase
    {
        public ResolverViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService, ModalNavigationStore modalNavigationStore, IServiceProvider serviceProvider) : base(signalStore, deviceStore, logService, modalNavigationStore, serviceProvider)
        {
        }

        public IEnumerable<AnalogSignal> AnalogSignals
        {
            get
            {
                var signals = SignalStore.GetSignals<AnalogSignal>(nameof(ResolverViewModel));
                return signals;
            }
        }
    }

    public class SPIViewModel : ViewModelBase
    {
        public SPIViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService, ModalNavigationStore modalNavigationStore, IServiceProvider serviceProvider) : base(signalStore, deviceStore, logService, modalNavigationStore, serviceProvider)
        {
        }
    }

    public class LinViewModel : ViewModelBase
    {
        public LinViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService, ModalNavigationStore modalNavigationStore, IServiceProvider serviceProvider) : base(signalStore, deviceStore, logService, modalNavigationStore, serviceProvider)
        {
        }
    }

    public class NXPFlashViewModel : ViewModelBase
    {
        public NXPFlashViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService, ModalNavigationStore modalNavigationStore, IServiceProvider serviceProvider) : base(signalStore, deviceStore, logService, modalNavigationStore, serviceProvider)
        {
        }
    }
}
