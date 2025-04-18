﻿using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ERad5TestGUI.Models;
using ERad5TestGUI.Services;
using ERad5TestGUI.Stores;

namespace ERad5TestGUI.ViewModels
{
    /// <summary>
    /// has discrete input signals and discrete output signals and NXPSignal
    /// </summary>
    public class NXPViewModel : SendFrameViewModelBase
    {
        private readonly ObservableCollection<DiscreteOutputSignal> _disOutputSignals = new ObservableCollection<DiscreteOutputSignal>();
        private readonly ObservableCollection<DiscreteInputSignal> _nxpSignals = new ObservableCollection<DiscreteInputSignal>();
        private readonly ObservableCollection<NXPInputSignal> _nxpInputSignals = new ObservableCollection<NXPInputSignal>();
        private RelayCommand locatorOutputsCommand;
        private RelayCommand locatorNxpCommand;

        public NXPViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService)
            :base(signalStore, deviceStore, logService)
        {

        }

        public NXPViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService, ModalNavigationStore modalNavigationStore, IServiceProvider serviceProvider) 
            : base(signalStore, deviceStore, logService, modalNavigationStore, serviceProvider)
        {
        }

        ~NXPViewModel()
        {
            //try
            //{
            //    SignalStore.SaveViewSignalLocator(nameof(NXPViewModel), _nxpInputSignals);
            //    SignalStore.SaveViewSignalLocator(nameof(NXPViewModel), _disOutputSignals, clear: false);
            //    SignalStore.SaveViewSignalLocator(nameof(NXPViewModel), _nxpSignals, clear: false);
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //}
        }

        public ObservableCollection<NXPInputSignal> NxpInputSignals => _nxpInputSignals;
        public ObservableCollection<DiscreteOutputSignal> DisOutputSignals => _disOutputSignals;
        public ObservableCollection<DiscreteInputSignal> NxpSignals => _nxpSignals;
        public ICommand LocatorOutputsCommand => locatorOutputsCommand ?? (locatorOutputsCommand = new RelayCommand(LocatorOutputSignals));
        public ICommand LocatorNxpCommand => locatorNxpCommand ?? (locatorNxpCommand = new RelayCommand(LocatorNXPSignals));

        public override void Init()
        {
            base.Init();

            _nxpInputSignals.AddRange(SignalStore.GetSignals<NXPInputSignal>(nameof(NXPViewModel)));
            _disOutputSignals.AddRange(SignalStore.GetSignals<DiscreteOutputSignal>(nameof(NXPViewModel)));
            _nxpSignals.AddRange(SignalStore.GetSignals<DiscreteInputSignal>(nameof(NXPViewModel)));

            //_outputSignals = SignalStore.GetObservableCollection<DiscreteOutputSignal>(nameof(DiscreteViewModel));
            //BuildFramesHelper = new DBCSignalBuildHelper(OutputSignals, SignalStore.DbcFile.Messages);
            //updateCommand = new RelayCommand(Update, () => OutputSignalSync);

            foreach (var item in DisOutputSignals)
            {
                item.PropertyChanged += Item_PropertyChanged;
            }
        }

        public override void LocatorSignals()
        {
            if (ModalNavigationStore != null)
            {
                var modalNavigationService = new ModalNavigationService<NXPInputSignalLocatorViewModel>(this.ModalNavigationStore, CreateLocatorInputViewModel);
                modalNavigationService.Navigate();
            }
            else
            {
                Views.DialogView dialogView = new Views.DialogView()
                {
                    Width = 850,
                    Height = 450
                };
                var deviceViewModel = CreateLocatorInputViewModel(dialogView);
                dialogView.DialogViewModel = deviceViewModel;
                dialogView.ShowDialog();
            }
        }

        public override void Send()
        {
            SendFD(SignalStore.BuildFrames(SignalStore.GetSignals<DiscreteOutputSignal>(nameof(NXPViewModel))));
        }

        #region Locator Signals
        private NXPInputSignalLocatorViewModel CreateLocatorInputViewModel() =>
            null;
        private NXPInputSignalLocatorViewModel CreateLocatorInputViewModel(System.Windows.Window window)
            => new NXPInputSignalLocatorViewModel(ViewName, _nxpInputSignals,
                                                       SignalStore,
                                                       CreateDisInSignal,
                                                       window);
        private NXPInputSignal CreateDisInSignal(Signal signal)
        {
            var existSignal = SignalStore.Signals.FirstOrDefault(x => x.Name == signal.SignalName && x.MessageID == signal.MessageID);
            if (existSignal != null && existSignal is NXPInputSignal analog)
                return analog;

            NXPInputSignal analogSignal = new NXPInputSignal(signal, nameof(NXPViewModel));
            SignalStore.AddSignal(analogSignal);
            return analogSignal;
        }

        private void LocatorOutputSignals()
        {
            if (ModalNavigationStore != null)
            {
                var modalNavigationService = new ModalNavigationService<DiscreteOutputSignalLocatorViewModel>(this.ModalNavigationStore, CreateDisOutLocatorViewModel);
                modalNavigationService.Navigate();
            }
            else
            {
                Views.DialogView dialogView = new Views.DialogView()
                {
                    Width = 850,
                    Height = 450
                };
                var deviceViewModel = CreateDisOutLocatorViewModel(dialogView);
                dialogView.DialogViewModel = deviceViewModel;
                dialogView.ShowDialog();
            }
        }

        private DiscreteOutputSignalLocatorViewModel CreateDisOutLocatorViewModel(System.Windows.Window window)
            =>  new DiscreteOutputSignalLocatorViewModel(ViewName, _disOutputSignals,
                                                        SignalStore,
                                                        CreateDisOutSignal, window);

        private DiscreteOutputSignalLocatorViewModel CreateDisOutLocatorViewModel()
            => new DiscreteOutputSignalLocatorViewModel(ViewName, new CloseModalNavigationService(ModalNavigationStore),
                                                        _disOutputSignals,
                                                        SignalStore,
                                                        CreateDisOutSignal);

        private DiscreteOutputSignal CreateDisOutSignal(Signal signal)
        {
            var existSignal = SignalStore.Signals.FirstOrDefault(x => x.Name == signal.SignalName && x.MessageID == signal.MessageID);
            if (existSignal != null && existSignal is DiscreteOutputSignal analog)
                return analog;

            DiscreteOutputSignal disOutSignal = new DiscreteOutputSignal(signal, nameof(NXPViewModel));

            //find a state 
            if (disOutSignal.SetStateSignal(SignalStore))
            {
                SignalStore.AddSignal(disOutSignal);
                return disOutSignal;
            }
            
            return null;
        }


        private void LocatorNXPSignals()
        {
            return;//Use Discrete Input Signal

            if (ModalNavigationStore != null)
            {
                //var modalNavigationService = new ModalNavigationService<DiscreteOutputSignalLocatorViewModel>(this.ModalNavigationStore, CreateDisOutLoactorViewModel);
                //modalNavigationService.Navigate();
            }
            else
            {
                Views.DialogView dialogView = new Views.DialogView()
                {
                    Width = 850,
                    Height = 450
                };
                var locatorViewModel = CreateNXPInLocatorViewModel(dialogView);
                dialogView.DialogViewModel = locatorViewModel;
                dialogView.ShowDialog();
            }
        }

        private NXPSignalLocatorViewModel CreateNXPInLocatorViewModel(System.Windows.Window window)
           => new NXPSignalLocatorViewModel(ViewName, null,
                                            SignalStore,
                                            CreateNXPInSignal, window);

        private NXPSignal CreateNXPInSignal(Signal signal)
        {
            var existSignal = SignalStore.Signals.FirstOrDefault(x => x.Name == signal.SignalName && x.MessageID == signal.MessageID);
            if (existSignal != null && existSignal is NXPSignal analog)
            {
                if (analog.ViewName.IndexOf(SignalBase.ReplaceViewModel(nameof(NXPViewModel))) < 0)
                    analog.ViewName += $";{SignalBase.ReplaceViewModel(nameof(NXPViewModel))}";
                return analog;
            }

            NXPSignal nxpSignal = new NXPSignal(signal, nameof(NXPViewModel));
            SignalStore.AddSignal(nxpSignal);
            return nxpSignal;
        }
        #endregion

        private void Update()
        {
            //update RealValue
            foreach (var item in DisOutputSignals)
            {
                if (item is DiscreteOutputSignal output)
                    output.UpdateRealValue();
            }
            Send();
        }

        private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SignalBase.OriginValue))
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
    }
}
