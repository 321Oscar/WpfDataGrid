using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ERad5TestGUI.Devices;
using ERad5TestGUI.Models;
using ERad5TestGUI.Services;
using ERad5TestGUI.Stores;

namespace ERad5TestGUI.ViewModels
{
    public class DiscreteViewModel : SendFrameViewModelBase
    {
        private readonly ObservableCollection<DiscreteOutputSignal> _outputSignals = new ObservableCollection<DiscreteOutputSignal>();
        private readonly ObservableCollection<DiscreteInputSignal> _inputSignals = new ObservableCollection<DiscreteInputSignal>();
        private bool _outputSignalSync;
        //private readonly DBCSignalBuildHelper dBCSignalBuildHelper;
        private RelayCommand _updateCommand;
        private RelayCommand _locatorOutputsCommand;
        private RelayCommand _sendBadAnwserCommand;
        private RelayCommand _disableSBCWWDTRIGCommand;
        private RelayCommand _clearTransitionsCommand;

        public DiscreteViewModel(SignalStore signalStore,
            DeviceStore deviceStore, 
            LogService logService, 
            ModalNavigationStore modalNavigationStore, 
            IServiceProvider serviceProvider)
            : base(signalStore, deviceStore, logService, modalNavigationStore, serviceProvider)
        {

        }

        public DiscreteViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService)
            : base(signalStore, deviceStore, logService)
        {

        }
        public ICommand UpdateCommand { get => _updateCommand ?? (_updateCommand = new RelayCommand(Update, () => OutputSignalSync)); }
        public ICommand LocatorOutputsCommand => _locatorOutputsCommand ?? (_locatorOutputsCommand = new RelayCommand(LocatorOutputSignals));
        public ICommand SendBadAnwserCommand => _sendBadAnwserCommand ?? (_sendBadAnwserCommand = new RelayCommand(SendBadAnswer));
        public ICommand DisableWWDTRIGCommand => _disableSBCWWDTRIGCommand ?? (_disableSBCWWDTRIGCommand = new RelayCommand(DisableSBCWD));
        public ICommand ClearTransitionsCommand => _clearTransitionsCommand ?? (_clearTransitionsCommand = new RelayCommand(ClearTransitions));

     

        public IEnumerable<DiscreteInputSignal> InputSignals => _inputSignals;
        public IEnumerable<DiscreteOutputSignal> OutputSignals => _outputSignals;
        public DiscreteOutputSignal DIS_SBC_WWD_TRIG => SignalStore.GetSignals<DiscreteOutputSignal>().FirstOrDefault(x => x.Name == "DIS_SBC_WWD_TRIG");
        public DiscreteOutputSignal SEND_BAD_ANSWER => SignalStore.GetSignals<DiscreteOutputSignal>().FirstOrDefault(x => x.Name == "SEND_BAD_ANSWER");

        public bool OutputSignalSync
        {
            get => _outputSignalSync;
            set
            {
                SetProperty(ref _outputSignalSync, value);
                _updateCommand.NotifyCanExecuteChanged();
                foreach (var item in OutputSignals)
                {
                    if (item is DiscreteOutputSignal output)
                        output.Sync = value;
                }
            }
        }

        //[RelayCommand(CanExecute = "OutputSignalSync")]

        public override void Init()
        {
            base.Init();
            foreach (var signal in SignalStore.GetSignals<DiscreteInputSignal>(nameof(DiscreteViewModel)))
            {
                _inputSignals.Add(signal);
            }
            foreach (var signal in SignalStore.GetSignals<DiscreteOutputSignal>(nameof(DiscreteViewModel)).Where(x => x.State != null))
            {
                _outputSignals.Add(signal);
            }

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

        public override void Send()
        {
            Send(SignalStore.BuildFrames(SignalStore.GetSignals<DiscreteOutputSignal>(nameof(DiscreteViewModel))));
        }
        /// <summary>
        /// locator input
        /// </summary>
        public override void LocatorSignals()
        {
            if (ModalNavigationStore != null)
            {
                var modalNavigationService = new ModalNavigationService<DiscreteInputSignalLocatorViewModel>(this.ModalNavigationStore, CreateLocatorInputViewModel);
                modalNavigationService.Navigate();
            }
            else
            {
                Views.DialogView dialogView = new Views.DialogView();
                var locatorViewModel = CreateLocatorInputViewModel(dialogView);
                dialogView.DialogViewModel = locatorViewModel;
                dialogView.ShowDialog();
            }
        }

        private void ClearTransitions()
        {
            //throw new NotImplementedException();
            foreach (var item in InputSignals)
            {
                item.ClearTransitions();
            }
        }

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

        private void DisableSBCWD()
        {
            DIS_SBC_WWD_TRIG.OriginValue = 1;
            Send();
            DIS_SBC_WWD_TRIG.OriginValue = 0;
        }

        private void SendBadAnswer()
        {
            SEND_BAD_ANSWER.OriginValue = 1;
            Send();
            SEND_BAD_ANSWER.OriginValue = 0;
        }

        private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SignalBase.OriginValue) && !OutputSignalSync)
            {
                if (sender is DiscreteOutputSignal outputSignal)
                {
                    if (outputSignal.OriginValue == outputSignal.State.OriginValue)
                    {
                        return;
                    }
                }

                Send();
            }
        }

      

        #region Signal Locator

        private DiscreteInputSignalLocatorViewModel CreateLocatorInputViewModel(System.Windows.Window window)
          => new DiscreteInputSignalLocatorViewModel(_inputSignals,
                                                      SignalStore,
                                                      CreateDisInSignal, window);

        private DiscreteInputSignalLocatorViewModel CreateLocatorInputViewModel() 
            => new DiscreteInputSignalLocatorViewModel(new CloseModalNavigationService(ModalNavigationStore),
                                                       _inputSignals,
                                                       SignalStore,
                                                       CreateDisInSignal);
        private DiscreteInputSignal CreateDisInSignal(Signal signal)
        {
            var existSignal = SignalStore.Signals.FirstOrDefault(x => x.Name == signal.SignalName && x.MessageID == signal.MessageID);
            if (existSignal != null && existSignal is DiscreteInputSignal analog)
                return analog;

            DiscreteInputSignal analogSignal = new DiscreteInputSignal(signal, nameof(DiscreteViewModel));
            SignalStore.AddSignal(analogSignal);
            return analogSignal;
        }

        private void LocatorOutputSignals()
        {
            if (ModalNavigationStore != null)
            {
                var modalNavigationService = new ModalNavigationService<DiscreteOutputSignalLocatorViewModel>(this.ModalNavigationStore, CreateDisOutLoactorViewModel);
                modalNavigationService.Navigate();
            }
            else
            {
                Views.DialogView dialogView = new Views.DialogView();
                var locatorViewModel = CreateDisOutLoactorViewModel(dialogView);
                dialogView.DialogViewModel = locatorViewModel;
                dialogView.ShowDialog();
            }

        }

        private DiscreteOutputSignalLocatorViewModel CreateDisOutLoactorViewModel(System.Windows.Window window)
         => new DiscreteOutputSignalLocatorViewModel(_outputSignals,
                                                     SignalStore,
                                                     CreateDisOutSignal, window);

        private DiscreteOutputSignalLocatorViewModel CreateDisOutLoactorViewModel() 
            => new DiscreteOutputSignalLocatorViewModel(new CloseModalNavigationService(ModalNavigationStore), 
                                                        _outputSignals, 
                                                        SignalStore,
                                                        CreateDisOutSignal);

        private DiscreteOutputSignal CreateDisOutSignal(Signal signal)
        {
            var existSignal = SignalStore.Signals.FirstOrDefault(x => x.Name == signal.SignalName && x.MessageID == signal.MessageID);
            if (existSignal != null && existSignal is DiscreteOutputSignal analog)
                return analog;

            DiscreteOutputSignal disOutSignal = new DiscreteOutputSignal(signal, nameof(DiscreteViewModel));
            
            SignalStore.AddSignal(disOutSignal);

            //find a state 
            if (disOutSignal.SetStateSignal(SignalStore))
            {
                return disOutSignal;
            }
            return null;
        }
        #endregion
    }
}