using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ERad5TestGUI.Models;
using ERad5TestGUI.Services;
using ERad5TestGUI.Stores;

namespace ERad5TestGUI.ViewModels
{
    public class DisConnectViewModel : SendFrameViewModelBase
    {
        private ObservableCollection<SignalBase> _inputSignals = new ObservableCollection<SignalBase>();
        private ObservableCollection<SignalBase> _outputSignals = new ObservableCollection<SignalBase>();
        private RelayCommand _updateCommand;

        public DisConnectViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService) : base(signalStore, deviceStore, logService)
        {

        }

        public DisConnectViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService, ModalNavigationStore modalNavigationStore, IServiceProvider serviceProvider) : base(signalStore, deviceStore, logService, modalNavigationStore, serviceProvider)
        {
        }

        public IEnumerable<SignalBase> InputSignals { get => _inputSignals; }
        public IEnumerable<SignalBase> OutputSignals { get => _outputSignals; }
        public ICommand UpdateCommand { get => _updateCommand ?? (_updateCommand = new RelayCommand(Update)); }

        public override void Init()
        {
            foreach (var signal in SignalStore.GetSignals<SignalBase>(nameof(DisConnectViewModel)).Where(x => x.InOrOut == false))
            {
                _inputSignals.Add(signal);
            }
            foreach (var signal in SignalStore.GetSignals<SignalBase>(nameof(DisConnectViewModel)).Where(x => x.InOrOut == true))
            {
                _outputSignals.Add(signal);
            }
        }

        private void Update()
        {
            Send(SignalStore.BuildFrames(OutputSignals));
        }

    }
}
