using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        private ObservableCollection<DiscreteOutputSignal> _outputSignals = new ObservableCollection<DiscreteOutputSignal>();
        private ObservableCollection<DiscreteInputSignal> _inputSignals = new ObservableCollection<DiscreteInputSignal>();
        //private readonly DBCSignalBuildHelper dBCSignalBuildHelper;
        private ICommand updateCommand;
        private RelayCommand locatorOutputsCommand;

        public DiscreteViewModel(SignalStore signalStore,
            DeviceStore deviceStore, 
            LogService logService, 
            ModalNavigationStore modalNavigationStore, 
            IServiceProvider serviceProvider)
            : base(signalStore, deviceStore, logService, modalNavigationStore, serviceProvider)
        {
            //_signalStore = signalStore;
            //this.logService = logService;
            
            
        }
        public ICommand LocatorOutputsCommand => locatorOutputsCommand ?? (locatorOutputsCommand = new RelayCommand(LocatorOutputSignals));
        public IEnumerable<DiscreteInputSignal> InputSignals => _inputSignals;
        public IEnumerable<DiscreteOutputSignal> OutputSignals => _outputSignals;

        public override void Init()
        {
            base.Init();
            foreach (var signal in SignalStore.SignalLocatorInfo.GetViewSignalInfo(nameof(DiscreteViewModel)).Signals.OfType<DiscreteInputSignal>())
            {
                _inputSignals.Add(signal);
            }
            foreach (var signal in SignalStore.SignalLocatorInfo.GetViewSignalInfo(nameof(DiscreteViewModel)).Signals.OfType<DiscreteOutputSignal>())
            {
                _outputSignals.Add(signal);
            }
            //_outputSignals = SignalStore.GetObservableCollection<DiscreteOutputSignal>(nameof(DiscreteViewModel));
            BuildFramesHelper = new DBCSignalBuildHelper(OutputSignals, SignalStore.DbcFile.Messages);
            updateCommand = new RelayCommand(Update, () => OutputSignalSync);

            foreach (var item in OutputSignals)
            {
                item.PropertyChanged += Item_PropertyChanged;
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            SignalStore.SaveViewSignalLocator(nameof(DiscreteViewModel), _inputSignals);
            SignalStore.SaveViewSignalLocator(nameof(DiscreteViewModel), _outputSignals, clear: false);
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
                    if (item is DiscreteOutputSignal output)
                        output.Sync = value;
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
                if (item is DiscreteOutputSignal output)
                    output.UpdateRealValue();
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

        public override void Send()
        {
            if (DeviceStore.HasDevice)
                DeviceStore.CurrentDevice.SendMultip(SignalStore.BuildFrames(SignalStore.GetSignals<DiscreteOutputSignal>(nameof(DiscreteViewModel))));
        }

        public override void LocatorSignals()
        {
            ModalNavigationService<DiscreteInputSignalLocatorViewModel> modalNavigationService =
                new ModalNavigationService<DiscreteInputSignalLocatorViewModel>(
                    this.ModalNavigationStore,
                    () => new DiscreteInputSignalLocatorViewModel(new CloseModalNavigationService(ModalNavigationStore), _inputSignals, SignalStore,
                    (signal) =>
                    {
                        var existSignal = SignalStore.Signals.FirstOrDefault(x => x.Name == signal.SignalName && x.MessageID == signal.MessageID);
                        if (existSignal != null && existSignal is DiscreteInputSignal analog)
                            return analog;

                        DiscreteInputSignal analogSignal = new DiscreteInputSignal()
                        {
                            Name = signal.SignalName,
                            StartBit = (int)signal.startBit,
                            Factor = signal.factor,
                            Offset = signal.offset,
                            ByteOrder = (int)signal.byteOrder,
                            Length = (int)signal.signalSize,
                            MessageID = signal.MessageID,

                        };
                        analogSignal.ViewName += "Discrete";
                        analogSignal.PinNumber = signal.Comment.GetCommentByKey("Pin_Number");
                      
                        SignalStore.AddSignal(analogSignal);
                        return analogSignal;
                    }));
            modalNavigationService.Navigate();
        }

        private void LocatorOutputSignals()
        {
            ModalNavigationService<DiscreteOutputSignalLocatorViewModel> modalNavigationService =
                new ModalNavigationService<DiscreteOutputSignalLocatorViewModel>(
                    this.ModalNavigationStore,
                    () => new DiscreteOutputSignalLocatorViewModel(new CloseModalNavigationService(ModalNavigationStore), _outputSignals, SignalStore,
                    (signal) =>
                    {
                        var existSignal = SignalStore.Signals.FirstOrDefault(x => x.Name == signal.SignalName && x.MessageID == signal.MessageID);
                        if (existSignal != null && existSignal is DiscreteOutputSignal analog)
                            return analog;

                        DiscreteOutputSignal disOutSignal = new DiscreteOutputSignal()
                        {
                            Name = signal.SignalName,
                            StartBit = (int)signal.startBit,
                            Factor = signal.factor,
                            Offset = signal.offset,
                            ByteOrder = (int)signal.byteOrder,
                            Length = (int)signal.signalSize,
                            MessageID = signal.MessageID,
                        };
                        disOutSignal.ViewName += "Discrete";
                        disOutSignal.PinNumber = signal.Comment.GetCommentByKey("Pin_Number");
                        SignalStore.AddSignal(disOutSignal);

                        //find a state 
                        if (disOutSignal.SetStateSignal(SignalStore))
                        {
                            return disOutSignal;
                        }
                        //var signalState = SignalStore.DBCSignals.FirstOrDefault(x => x.SignalName == signal.SignalName + "_State");
                        //if(signalState != null)
                        //{
                        //    var existInput = SignalStore.Signals.FirstOrDefault(x => x.Name == signal.SignalName + "_State");
                        //    if (existInput != null && existInput is DiscreteInputSignal exsitDiscrete)
                        //    {
                        //        analogSignal.State = exsitDiscrete;
                        //    }
                        //    else
                        //    {
                        //        var input = new DiscreteInputSignal()
                        //        {
                        //            Name = signalState.SignalName,
                        //            StartBit = (int)signalState.startBit,
                        //            Factor = signalState.factor,
                        //            Offset = signalState.offset,
                        //            ByteOrder = (int)signalState.byteOrder,
                        //            Length = (int)signalState.signalSize,
                        //            MessageID = signalState.MessageID,
                        //        };
                        //        analogSignal.State = input;
                        //    }
                        //}
                        return null;
                    }));
            modalNavigationService.Navigate();
        }
    }
}