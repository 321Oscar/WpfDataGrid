using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using WpfApp1.Devices;
using WpfApp1.Models;
using WpfApp1.Services;
using WpfApp1.Stores;

namespace WpfApp1.ViewModels
{
    public class DiscreteViewModel : SendFrameViewModelBase
    {
        private bool outputSignalSync;
        private ObservableCollection<DiscreteOutputSignal> _outputSignals;
        //private readonly DBCSignalBuildHelper dBCSignalBuildHelper;
        private ICommand updateCommand;

        public DiscreteViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService)
            : base(signalStore, deviceStore, logService)
        {
            //_signalStore = signalStore;
            //this.logService = logService;
            BuildFramesHelper = new DBCSignalBuildHelper(OutputSignals, signalStore.DbcFile.Messages);
            
        }

        public IEnumerable<DiscreteInputSignal> InputSignals => SignalStore.GetSignals<DiscreteInputSignal>(nameof(DiscreteViewModel));
        public IEnumerable<DiscreteOutputSignal> OutputSignals => _outputSignals;

        public override void Init()
        {
            base.Init();
            _outputSignals = SignalStore.GetObservableCollection<DiscreteOutputSignal>(nameof(DiscreteViewModel));

            updateCommand = new RelayCommand(Update, () => OutputSignalSync);

            foreach (var item in OutputSignals)
            {
                item.PropertyChanged += Item_PropertyChanged;
            }
        }

        protected override void OnActivated()
        {
            base.OnActivated();
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();
            foreach (var item in OutputSignals)
            {
                item.PropertyChanged -= Item_PropertyChanged;
            }
        }
        [Obsolete]
        protected override void CurrentDevice_OnMsgReceived(IEnumerable<IFrame> frames)
        {
            //foreach (var signal in SignalStore.ParseMsgsYield(frames, InputSignals))
            //{
            //    if (signal != null)
            //        LogService.Info($"{signal.Name}:{signal.RealValue}");
            //}
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

        public ICommand UpdateCommand { get => updateCommand; }
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
            if (e.PropertyName == nameof(SignalBase.OriginValue) && !OutputSignalSync)
            {
                Send();
            }
        }
    }
}