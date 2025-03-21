﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ERad5TestGUI.Services;
using ERad5TestGUI.Stores;

namespace ERad5TestGUI.ViewModels
{
    public class MemoryViewModel : ViewModelBase
    {
        private ICommand _loadSrecFileCommand;
        private readonly ObservableCollection<UDS.SRecord.SrecDataOnly> _s19Records = new ObservableCollection<UDS.SRecord.SrecDataOnly>();
        public MemoryViewModel(DeviceStore deviceStore, LogService logService) : base(null, deviceStore, logService)
        {
            S19Records = new ReadOnlyObservableCollection<UDS.SRecord.SrecDataOnly>(_s19Records);
        }
        public MemoryViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService, ModalNavigationStore modalNavigationStore, IServiceProvider serviceProvider) 
            : base(signalStore, deviceStore, logService, modalNavigationStore, serviceProvider)
        {
        }

        public ICommand LoadSrecFileCommand => _loadSrecFileCommand ?? (_loadSrecFileCommand = new AsyncRelayCommand(LoadSrecFile));
        public ReadOnlyObservableCollection<UDS.SRecord.SrecDataOnly> S19Records { get; set; }
        private Task LoadSrecFile()
        {
            var ofd = new CommonOpenFileDialog();
            ofd.Filters.Add(new CommonFileDialogFilter("srec file", "*.srec;*.s19;*.s28;*.s37"));
            ofd.Filters.Add(new CommonFileDialogFilter("all", "*.*"));

            if (ofd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                return Task.Run(() =>
                {
                    //S19RecordFile s19File = new S19RecordFile();
                    //S19RecordFile.ParseS19File(ofd.FileName, S19Records);
                    IsLoading = true;
                    UDS.SRecord.SrecFileOnlyData f = new UDS.SRecord.SrecFileOnlyData(ofd.FileName);
                    Dispatch(() =>
                    {
                        _s19Records.Clear();
                        foreach (var sData in f.Content)
                        {
                            _s19Records.Add(sData);
                        }
                    });
                    return;
                }).ContinueWith((x) =>
                {
                    IsLoading = false;
                });
            }
            else
            {
                return Task.CompletedTask;
            }
        }
    }

    public class NXPFlashViewModel : ViewModelBase
    {
        public NXPFlashViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService, ModalNavigationStore modalNavigationStore, IServiceProvider serviceProvider) : base(signalStore, deviceStore, logService, modalNavigationStore, serviceProvider)
        {
        }
    }
}
