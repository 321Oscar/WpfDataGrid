using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Windows.Input;
using WpfApp1.Devices;
using WpfApp1.Models;
using WpfApp1.Services;
using WpfApp1.Stores;

namespace WpfApp1.ViewModels
{
    public class DiscreteViewModel : ViewModelBase
    {
        private bool outputSignalSync;
        private readonly DBCSignalBuildHelper dBCSignalBuildHelper;

        public DiscreteViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService)
            : base(signalStore, deviceStore, logService)
        {
            //_signalStore = signalStore;
            //this.logService = logService;
            dBCSignalBuildHelper = new DBCSignalBuildHelper(OutputSignals, signalStore.DbcFile.Messages);
            UpdateCommand = new RelayCommand(Update,() => OutputSignalSync);
        }

        public IEnumerable<DiscreteInputSignal> InputSignals => SignalStore.GetSignals<DiscreteInputSignal>();
        public IEnumerable<DiscreteOutputSignal> OutputSignals => SignalStore.GetObservableCollection<DiscreteOutputSignal>();

        protected override void OnActivated()
        {
            base.OnActivated();
            foreach (var item in OutputSignals)
            {
                item.PropertyChanged += Item_PropertyChanged;
            }
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();
            foreach (var item in OutputSignals)
            {
                item.PropertyChanged -= Item_PropertyChanged;
            }
        }

        protected override void CurrentDevice_OnMsgReceived(IEnumerable<IFrame> frames)
        {
            foreach (var signal in SignalStore.ParseMsgsYield(frames, InputSignals))
            {
                if (signal != null)
                    LogService.Info($"{signal.Name}:{signal.RealValue}");
            }
        }

        public bool OutputSignalSync 
        {
            get => outputSignalSync;
            set 
            { 
                SetProperty(ref outputSignalSync, value);
                (UpdateCommand as IRelayCommand).NotifyCanExecuteChanged();
                foreach (var item in OutputSignals)
                {
                    item.Sync = value;
                }
            }
        }

        public ICommand UpdateCommand { get; }
        //[RelayCommand(CanExecute = "OutputSignalSync")]
        private void Update()
        {
            //update RealValue
            foreach (var item in OutputSignals)
            {
                item.UpdateRealValue();
            }
            Send();
        }

        private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SignalBase.RealValue) && !OutputSignalSync)
            {
                Send();
            }
        }

        private void Send()
        {
            if (DeviceStore.HasDevice)
            {
                DeviceStore.CurrentDevice.SendMultip(dBCSignalBuildHelper.BuildFrames());
            }
        }
    }
}